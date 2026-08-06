-- Booking Management: full booking lifecycle (online/walk-in booking,
-- modify/cancel/extend, approval workflow), coupons/promo codes, tax
-- config (via the existing settings table), booking extensions, and a
-- waitlist. Booking Timeline reuses the existing audit_logs table
-- (entity_type = 'Booking') rather than a new history table.

alter table public.bookings
    add column if not exists branch_id uuid references public.branches(id) on delete restrict,
    add column if not exists return_branch_id uuid references public.branches(id) on delete restrict,
    add column if not exists booking_type text not null default 'Online',
    add column if not exists subtotal_amount numeric(18,2) not null default 0,
    add column if not exists discount_amount numeric(18,2) not null default 0,
    add column if not exists tax_amount numeric(18,2) not null default 0,
    add column if not exists coupon_id uuid,
    add column if not exists cancelled_at timestamptz,
    add column if not exists cancellation_reason text,
    add column if not exists approved_by uuid references public.users(id) on delete set null,
    add column if not exists approved_at timestamptz,
    add column if not exists created_by uuid references public.users(id) on delete set null;

-- Backfill subtotal for existing rows so historical bookings still add up
-- (subtotal + tax - discount = total, with discount/tax defaulting to 0 above).
update public.bookings set subtotal_amount = total_amount where subtotal_amount = 0;

-- The original coupons table (code, discount_amount, is_active) was never
-- wired to the application; replace it with the full model.
drop table if exists public.coupons cascade;

create table public.coupons (
    id uuid primary key default gen_random_uuid(),
    code text not null unique,
    description text,
    discount_type text not null default 'Percentage',
    discount_value numeric(18,2) not null,
    min_booking_amount numeric(18,2),
    max_discount_amount numeric(18,2),
    start_date timestamptz,
    end_date timestamptz,
    usage_limit int,
    usage_count int not null default 0,
    is_active boolean not null default true,
    created_at timestamptz not null default now()
);

alter table public.bookings
    add constraint fk_bookings_coupon foreign key (coupon_id) references public.coupons(id) on delete restrict;

create table if not exists public.coupon_redemptions (
    id uuid primary key default gen_random_uuid(),
    coupon_id uuid not null references public.coupons(id) on delete restrict,
    booking_id uuid not null references public.bookings(id) on delete cascade,
    customer_id uuid not null references public.users(id) on delete restrict,
    discount_applied numeric(18,2) not null,
    redeemed_at timestamptz not null default now()
);

create table if not exists public.booking_extensions (
    id uuid primary key default gen_random_uuid(),
    booking_id uuid not null references public.bookings(id) on delete cascade,
    previous_end_date timestamptz not null,
    new_end_date timestamptz not null,
    additional_amount numeric(18,2) not null,
    status text not null default 'Approved',
    reason text,
    requested_by uuid not null references public.users(id) on delete restrict,
    requested_at timestamptz not null default now(),
    decided_at timestamptz
);

create table if not exists public.waitlist_entries (
    id uuid primary key default gen_random_uuid(),
    customer_id uuid not null references public.users(id) on delete cascade,
    vehicle_id uuid references public.vehicles(id) on delete cascade,
    vehicle_category_id uuid references public.vehicle_categories(id) on delete cascade,
    branch_id uuid references public.branches(id) on delete set null,
    desired_start_date timestamptz not null,
    desired_end_date timestamptz not null,
    status text not null default 'Waiting',
    created_at timestamptz not null default now(),
    notified_at timestamptz
);

-- Tax rate lives in the existing, previously-unwired settings table.
insert into public.settings (key_name, key_value, category)
values ('TaxRatePercent', '8', 'Billing')
on conflict (key_name) do nothing;

insert into public.settings (key_name, key_value, category)
values ('RequireBookingApproval', 'true', 'Booking')
on conflict (key_name) do nothing;

-- Hard double-booking prevention at the database layer: no two
-- non-cancelled/rejected/no-show bookings may hold overlapping date
-- ranges on the same vehicle, even under concurrent requests.
create extension if not exists btree_gist;

alter table public.bookings
    add constraint bookings_no_overlap
    exclude using gist (
        vehicle_id with =,
        daterange(start_date::date, end_date::date, '[)') with &&
    ) where (status not in ('Cancelled', 'Rejected', 'NoShow'));

create index if not exists idx_bookings_branch_id on public.bookings(branch_id);
create index if not exists idx_bookings_coupon_id on public.bookings(coupon_id);
create index if not exists idx_bookings_status on public.bookings(status);
create index if not exists idx_coupon_redemptions_coupon_id on public.coupon_redemptions(coupon_id);
create index if not exists idx_coupon_redemptions_booking_id on public.coupon_redemptions(booking_id);
create index if not exists idx_booking_extensions_booking_id on public.booking_extensions(booking_id);
create index if not exists idx_waitlist_entries_customer_id on public.waitlist_entries(customer_id);
create index if not exists idx_waitlist_entries_status on public.waitlist_entries(status);
