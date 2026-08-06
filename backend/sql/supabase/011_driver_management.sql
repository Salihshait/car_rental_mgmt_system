-- Driver Management: fuller driver profile, license document tracking,
-- attendance, salary payslips, and internal performance ratings. Builds on
-- top of the `drivers` table wired up in Fleet Management.

alter table public.drivers
    add column if not exists photo_url text,
    add column if not exists address text,
    add column if not exists emergency_contact_name text,
    add column if not exists emergency_contact_phone text,
    add column if not exists date_of_joining timestamptz,
    add column if not exists employment_status text not null default 'Active',
    add column if not exists department_id uuid references public.departments(id) on delete set null,
    add column if not exists branch_id uuid references public.branches(id) on delete set null,
    add column if not exists license_expiry_date timestamptz,
    add column if not exists base_salary numeric(18,2);

create table if not exists public.driver_documents (
    id uuid primary key default gen_random_uuid(),
    driver_id uuid not null references public.drivers(id) on delete cascade,
    document_type text not null,
    document_number text,
    expiry_date timestamptz,
    storage_path text,
    verification_status text not null default 'Pending',
    uploaded_at timestamptz not null default now()
);

create table if not exists public.driver_attendance (
    id uuid primary key default gen_random_uuid(),
    driver_id uuid not null references public.drivers(id) on delete cascade,
    attendance_date date not null,
    check_in_at timestamptz,
    check_out_at timestamptz,
    status text not null default 'Present',
    notes text,
    created_at timestamptz not null default now()
);

create unique index if not exists idx_driver_attendance_driver_date on public.driver_attendance(driver_id, attendance_date);

create table if not exists public.driver_salary_payments (
    id uuid primary key default gen_random_uuid(),
    driver_id uuid not null references public.drivers(id) on delete cascade,
    period_start timestamptz not null,
    period_end timestamptz not null,
    base_amount numeric(18,2) not null,
    deductions numeric(18,2) not null default 0,
    bonus numeric(18,2) not null default 0,
    net_amount numeric(18,2) not null,
    status text not null default 'Pending',
    paid_at timestamptz,
    notes text,
    created_by uuid not null references public.users(id) on delete restrict,
    created_at timestamptz not null default now()
);

create table if not exists public.driver_ratings (
    id uuid primary key default gen_random_uuid(),
    driver_id uuid not null references public.drivers(id) on delete cascade,
    rated_by uuid not null references public.users(id) on delete restrict,
    score int not null check (score between 1 and 5),
    category text not null default 'Overall',
    comment text,
    created_at timestamptz not null default now()
);

create index if not exists idx_driver_documents_driver_id on public.driver_documents(driver_id);
create index if not exists idx_driver_attendance_driver_id on public.driver_attendance(driver_id);
create index if not exists idx_driver_salary_payments_driver_id on public.driver_salary_payments(driver_id);
create index if not exists idx_driver_ratings_driver_id on public.driver_ratings(driver_id);
