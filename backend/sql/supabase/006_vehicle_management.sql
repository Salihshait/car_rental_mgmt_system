-- Vehicle Management: unified vehicle_documents (supersedes the standalone
-- insurance table), brand/model FKs replacing free-text make/model, status
-- lifecycle replacing is_available, and vehicle metadata additions.

create table if not exists public.vehicle_documents (
    id uuid primary key default gen_random_uuid(),
    vehicle_id uuid not null references public.vehicles(id) on delete cascade,
    document_type text not null,
    document_number text,
    issued_by text,
    expiry_date timestamptz,
    storage_path text,
    uploaded_at timestamptz not null default now()
);

alter table public.vehicles
    drop column if exists make,
    drop column if exists model,
    drop column if exists is_available,
    add column if not exists brand_id uuid references public.brands(id) on delete set null,
    add column if not exists model_id uuid references public.models(id) on delete set null,
    add column if not exists color text,
    add column if not exists engine_number text,
    add column if not exists gps_device_id text,
    add column if not exists status text not null default 'Available',
    add column if not exists created_at timestamptz not null default now(),
    add column if not exists updated_at timestamptz;

-- Migrate existing insurance records into the unified documents table before dropping it
insert into public.vehicle_documents (vehicle_id, document_type, document_number, issued_by, expiry_date, uploaded_at)
select vehicle_id, 'Insurance', policy_number, provider, expiry_date, now()
from public.insurance;

drop table if exists public.insurance;
