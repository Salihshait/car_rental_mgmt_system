-- Customer Management: blacklist/corporate/emergency-contact fields on the
-- existing customers table, plus new customer_documents and
-- favorite_vehicles tables. The existing (unused) notifications table needs
-- no schema change, only EF wiring.

alter table public.customers
    add column if not exists is_blacklisted boolean not null default false,
    add column if not exists is_corporate boolean not null default false,
    add column if not exists company_name text,
    add column if not exists emergency_contact_name text,
    add column if not exists emergency_contact_phone text,
    add column if not exists emergency_contact_relation text;

create table if not exists public.customer_documents (
    id uuid primary key default gen_random_uuid(),
    customer_id uuid not null references public.users(id) on delete cascade,
    document_type text not null,
    document_number text,
    expiry_date timestamptz,
    storage_path text,
    verification_status text not null default 'Pending',
    uploaded_at timestamptz not null default now()
);

create table if not exists public.favorite_vehicles (
    customer_id uuid not null references public.users(id) on delete cascade,
    vehicle_id uuid not null references public.vehicles(id) on delete cascade,
    created_at timestamptz not null default now(),
    primary key (customer_id, vehicle_id)
);
