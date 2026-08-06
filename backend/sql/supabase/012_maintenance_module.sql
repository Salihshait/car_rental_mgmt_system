-- Maintenance Module: workshops/vendors, spare parts inventory, warranty
-- and AMC contract tracking, compliance inspections, and a standalone
-- expense ledger, all layered on top of the vehicle_maintenance table
-- wired up in Fleet Management.

create table if not exists public.vendors (
    id uuid primary key default gen_random_uuid(),
    name text not null,
    vendor_type text not null default 'Workshop',
    contact_name text,
    phone text,
    email text,
    address text,
    is_active boolean not null default true,
    created_at timestamptz not null default now()
);

create table if not exists public.workshops (
    id uuid primary key default gen_random_uuid(),
    name text not null,
    vendor_id uuid references public.vendors(id) on delete set null,
    address text,
    phone text,
    is_active boolean not null default true,
    created_at timestamptz not null default now()
);

alter table public.vehicle_maintenance
    add column if not exists workshop_id uuid references public.workshops(id) on delete set null,
    add column if not exists maintenance_type text not null default 'Scheduled';

create table if not exists public.spare_parts (
    id uuid primary key default gen_random_uuid(),
    part_number text not null unique,
    name text not null,
    category text,
    unit_cost numeric(18,2) not null default 0,
    stock_quantity int not null default 0,
    reorder_level int not null default 0,
    preferred_vendor_id uuid references public.vendors(id) on delete set null,
    created_at timestamptz not null default now()
);

create table if not exists public.maintenance_part_usage (
    id uuid primary key default gen_random_uuid(),
    maintenance_id uuid not null references public.vehicle_maintenance(id) on delete cascade,
    spare_part_id uuid not null references public.spare_parts(id) on delete restrict,
    quantity int not null,
    unit_cost numeric(18,2) not null,
    total_amount numeric(18,2) not null,
    created_at timestamptz not null default now()
);

create table if not exists public.vehicle_warranties (
    id uuid primary key default gen_random_uuid(),
    vehicle_id uuid not null references public.vehicles(id) on delete cascade,
    warranty_type text not null,
    provider text,
    start_date timestamptz not null,
    end_date timestamptz not null,
    coverage_details text,
    status text not null default 'Active',
    created_at timestamptz not null default now()
);

create table if not exists public.amc_contracts (
    id uuid primary key default gen_random_uuid(),
    vehicle_id uuid not null references public.vehicles(id) on delete cascade,
    vendor_id uuid not null references public.vendors(id) on delete restrict,
    contract_number text not null,
    start_date timestamptz not null,
    end_date timestamptz not null,
    coverage_details text,
    cost numeric(18,2) not null default 0,
    status text not null default 'Active',
    created_at timestamptz not null default now()
);

create table if not exists public.vehicle_inspections (
    id uuid primary key default gen_random_uuid(),
    vehicle_id uuid not null references public.vehicles(id) on delete cascade,
    inspection_type text not null,
    inspection_date timestamptz not null,
    next_due_date timestamptz,
    result text not null default 'Pass',
    inspector_name text,
    vendor_id uuid references public.vendors(id) on delete set null,
    notes text,
    certificate_url text,
    created_at timestamptz not null default now()
);

create table if not exists public.maintenance_expenses (
    id uuid primary key default gen_random_uuid(),
    vehicle_id uuid references public.vehicles(id) on delete set null,
    maintenance_id uuid references public.vehicle_maintenance(id) on delete set null,
    vendor_id uuid references public.vendors(id) on delete set null,
    category text not null default 'Other',
    amount numeric(18,2) not null,
    description text,
    expense_date timestamptz not null default now(),
    created_by uuid not null references public.users(id) on delete restrict,
    created_at timestamptz not null default now()
);

create index if not exists idx_workshops_vendor_id on public.workshops(vendor_id);
create index if not exists idx_vehicle_maintenance_workshop_id on public.vehicle_maintenance(workshop_id);
create index if not exists idx_spare_parts_preferred_vendor_id on public.spare_parts(preferred_vendor_id);
create index if not exists idx_maintenance_part_usage_maintenance_id on public.maintenance_part_usage(maintenance_id);
create index if not exists idx_maintenance_part_usage_spare_part_id on public.maintenance_part_usage(spare_part_id);
create index if not exists idx_vehicle_warranties_vehicle_id on public.vehicle_warranties(vehicle_id);
create index if not exists idx_amc_contracts_vehicle_id on public.amc_contracts(vehicle_id);
create index if not exists idx_amc_contracts_vendor_id on public.amc_contracts(vendor_id);
create index if not exists idx_vehicle_inspections_vehicle_id on public.vehicle_inspections(vehicle_id);
create index if not exists idx_maintenance_expenses_vehicle_id on public.maintenance_expenses(vehicle_id);
create index if not exists idx_maintenance_expenses_maintenance_id on public.maintenance_expenses(maintenance_id);
