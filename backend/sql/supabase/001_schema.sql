create extension if not exists pgcrypto;

create table if not exists public.roles (
    id uuid primary key default gen_random_uuid(),
    name text not null unique,
    description text,
    is_system boolean not null default false,
    created_at timestamptz not null default now()
);

create table if not exists public.users (
    id uuid primary key default gen_random_uuid(),
    first_name text not null,
    last_name text not null,
    email text not null unique,
    phone_number text,
    password_hash text not null,
    refresh_token text,
    refresh_token_expiry_time timestamptz,
    is_email_verified boolean not null default false,
    is_active boolean not null default true,
    role_id uuid not null references public.roles(id) on delete restrict,
    created_at timestamptz not null default now(),
    updated_at timestamptz
);

create table if not exists public.branches (
    id uuid primary key default gen_random_uuid(),
    name text not null unique,
    city text not null,
    state text not null,
    country text not null,
    address text not null,
    is_active boolean not null default true,
    created_at timestamptz not null default now()
);

create table if not exists public.vehicles (
    id uuid primary key default gen_random_uuid(),
    registration_number text not null unique,
    vin text not null,
    make text not null,
    model text not null,
    year int not null,
    fuel_type text not null,
    transmission text not null,
    seating_capacity int not null,
    daily_rate numeric(18,2) not null,
    is_available boolean not null default true,
    branch_id uuid not null references public.branches(id) on delete restrict
);

create table if not exists public.bookings (
    id uuid primary key default gen_random_uuid(),
    customer_id uuid not null references public.users(id) on delete restrict,
    vehicle_id uuid not null references public.vehicles(id) on delete restrict,
    booking_date timestamptz not null default now(),
    start_date timestamptz not null,
    end_date timestamptz not null,
    total_amount numeric(18,2) not null,
    status text not null,
    notes text
);

create table if not exists public.payments (
    id uuid primary key default gen_random_uuid(),
    booking_id uuid not null references public.bookings(id) on delete restrict,
    amount numeric(18,2) not null,
    payment_method text not null default 'Card',
    gateway text not null default 'Razorpay',
    status text not null default 'Verified',
    transaction_reference text,
    paid_at timestamptz,
    created_at timestamptz not null default now()
);

create table if not exists public.permissions (
    id uuid primary key default gen_random_uuid(),
    name text not null unique,
    description text
);

create table if not exists public.role_permissions (
    role_id uuid not null references public.roles(id) on delete cascade,
    permission_id uuid not null references public.permissions(id) on delete cascade,
    primary key (role_id, permission_id)
);

create table if not exists public.customers (
    id uuid primary key default gen_random_uuid(),
    user_id uuid not null unique references public.users(id) on delete cascade,
    kyc_status text not null default 'Pending',
    wallet_balance numeric(18,2) not null default 0,
    loyalty_points int not null default 0,
    created_at timestamptz not null default now()
);

create table if not exists public.drivers (
    id uuid primary key default gen_random_uuid(),
    user_id uuid not null unique references public.users(id) on delete cascade,
    license_number text not null unique,
    kyc_status text not null default 'Pending',
    rating numeric(3,2),
    created_at timestamptz not null default now()
);

create table if not exists public.vehicle_categories (
    id uuid primary key default gen_random_uuid(),
    name text not null unique,
    description text
);

create table if not exists public.brands (
    id uuid primary key default gen_random_uuid(),
    name text not null unique
);

create table if not exists public.models (
    id uuid primary key default gen_random_uuid(),
    brand_id uuid not null references public.brands(id) on delete restrict,
    name text not null,
    category_id uuid not null references public.vehicle_categories(id) on delete restrict
);

create table if not exists public.refunds (
    id uuid primary key default gen_random_uuid(),
    booking_id uuid not null references public.bookings(id) on delete restrict,
    amount numeric(18,2) not null,
    status text not null,
    processed_at timestamptz
);

create table if not exists public.coupons (
    id uuid primary key default gen_random_uuid(),
    code text not null unique,
    discount_amount numeric(18,2) not null,
    is_active boolean not null default true
);

create table if not exists public.vehicle_maintenance (
    id uuid primary key default gen_random_uuid(),
    vehicle_id uuid not null references public.vehicles(id) on delete restrict,
    service_type text not null,
    scheduled_on timestamptz not null,
    status text not null,
    cost numeric(18,2)
);

create table if not exists public.insurance (
    id uuid primary key default gen_random_uuid(),
    vehicle_id uuid not null references public.vehicles(id) on delete restrict,
    policy_number text not null unique,
    expiry_date timestamptz not null,
    provider text not null
);

create table if not exists public.fuel_logs (
    id uuid primary key default gen_random_uuid(),
    vehicle_id uuid not null references public.vehicles(id) on delete restrict,
    logged_on timestamptz not null,
    quantity numeric(18,2) not null,
    cost numeric(18,2) not null
);

create table if not exists public.documents (
    id uuid primary key default gen_random_uuid(),
    entity_type text not null,
    entity_id uuid not null,
    document_type text not null,
    storage_path text not null,
    uploaded_at timestamptz not null default now()
);

create table if not exists public.reviews (
    id uuid primary key default gen_random_uuid(),
    customer_id uuid not null references public.customers(id) on delete restrict,
    vehicle_id uuid not null references public.vehicles(id) on delete restrict,
    rating int not null,
    comment text,
    created_at timestamptz not null default now()
);

create table if not exists public.notifications (
    id uuid primary key default gen_random_uuid(),
    user_id uuid not null references public.users(id) on delete cascade,
    notification_type text not null,
    message text not null,
    is_read boolean not null default false,
    created_at timestamptz not null default now()
);

create table if not exists public.audit_logs (
    id uuid primary key default gen_random_uuid(),
    user_id uuid references public.users(id) on delete set null,
    action text not null,
    entity_type text not null,
    entity_id uuid,
    payload jsonb,
    created_at timestamptz not null default now()
);

create table if not exists public.settings (
    id uuid primary key default gen_random_uuid(),
    key_name text not null unique,
    key_value text not null,
    category text not null,
    updated_at timestamptz not null default now()
);

create index if not exists idx_users_role_id on public.users(role_id);
create index if not exists idx_vehicles_branch_id on public.vehicles(branch_id);
create index if not exists idx_bookings_customer_id on public.bookings(customer_id);
create index if not exists idx_bookings_vehicle_id on public.bookings(vehicle_id);
create index if not exists idx_payments_booking_id on public.payments(booking_id);
create index if not exists idx_notifications_user_id on public.notifications(user_id);
create index if not exists idx_audit_logs_user_id on public.audit_logs(user_id);

-- Optional: enable Row Level Security for Supabase auth usage.
-- This keeps the schema ready for auth-oriented access policies.
alter table public.roles enable row level security;
alter table public.users enable row level security;
alter table public.branches enable row level security;
alter table public.vehicles enable row level security;
alter table public.bookings enable row level security;
alter table public.payments enable row level security;
alter table public.customers enable row level security;
alter table public.drivers enable row level security;
alter table public.notifications enable row level security;
alter table public.audit_logs enable row level security;
