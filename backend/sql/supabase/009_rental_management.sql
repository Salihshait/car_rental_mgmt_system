-- Rental Management: the operational handover of a vehicle (pickup/return),
-- signed agreement, fuel/odometer readings, damage inspection with photos,
-- late-return and extra-charge billing, and security deposit tracking.
-- Wires up the previously-unused refunds table for deposit refunds instead
-- of adding a new one.

alter table public.payments
    add column if not exists purpose text not null default 'RentalPayment';

create table if not exists public.rentals (
    id uuid primary key default gen_random_uuid(),
    booking_id uuid not null references public.bookings(id) on delete restrict,
    status text not null default 'Active',

    pickup_at timestamptz not null default now(),
    pickup_odometer_reading int not null,
    pickup_fuel_level_percent numeric(5,2) not null,
    pickup_condition_notes text,
    pickup_staff_user_id uuid not null references public.users(id) on delete restrict,

    agreement_signed_at timestamptz,
    agreement_signature_url text,
    agreement_pdf_url text,

    return_at timestamptz,
    return_odometer_reading int,
    return_fuel_level_percent numeric(5,2),
    return_condition_notes text,
    return_staff_user_id uuid references public.users(id) on delete set null,

    security_deposit_amount numeric(18,2) not null default 0,
    security_deposit_payment_id uuid references public.payments(id) on delete set null,
    security_deposit_status text not null default 'Held',
    security_deposit_refund_amount numeric(18,2),

    late_fee_amount numeric(18,2) not null default 0,
    late_hours int not null default 0,
    final_invoice_id uuid references public.invoices(id) on delete set null,

    created_at timestamptz not null default now()
);

create unique index if not exists idx_rentals_booking_id on public.rentals(booking_id);
create index if not exists idx_rentals_status on public.rentals(status);

create table if not exists public.rental_damages (
    id uuid primary key default gen_random_uuid(),
    rental_id uuid not null references public.rentals(id) on delete cascade,
    stage text not null default 'Pickup',
    description text not null,
    severity text not null default 'Minor',
    estimated_repair_cost numeric(18,2) not null default 0,
    reported_by uuid not null references public.users(id) on delete restrict,
    created_at timestamptz not null default now()
);

create index if not exists idx_rental_damages_rental_id on public.rental_damages(rental_id);

create table if not exists public.rental_photos (
    id uuid primary key default gen_random_uuid(),
    rental_id uuid not null references public.rentals(id) on delete cascade,
    stage text not null default 'Pickup',
    category text not null default 'Other',
    storage_url text not null,
    rental_damage_id uuid references public.rental_damages(id) on delete set null,
    uploaded_by uuid not null references public.users(id) on delete restrict,
    uploaded_at timestamptz not null default now()
);

create index if not exists idx_rental_photos_rental_id on public.rental_photos(rental_id);

create table if not exists public.rental_charges (
    id uuid primary key default gen_random_uuid(),
    rental_id uuid not null references public.rentals(id) on delete cascade,
    charge_type text not null default 'Other',
    description text,
    amount numeric(18,2) not null,
    created_at timestamptz not null default now()
);

create index if not exists idx_rental_charges_rental_id on public.rental_charges(rental_id);

-- Late-fee and deposit policy, same key/value settings table used by
-- Booking Management for TaxRatePercent / RequireBookingApproval.
insert into public.settings (key_name, key_value, category)
values ('LateFeeGraceMinutes', '60', 'Rental')
on conflict (key_name) do nothing;

insert into public.settings (key_name, key_value, category)
values ('LateFeeDailyRateMultiplier', '1.5', 'Rental')
on conflict (key_name) do nothing;

insert into public.settings (key_name, key_value, category)
values ('DefaultSecurityDepositAmount', '100', 'Rental')
on conflict (key_name) do nothing;

create index if not exists idx_refunds_booking_id on public.refunds(booking_id);
