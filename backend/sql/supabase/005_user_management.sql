-- User Management + RBAC: departments, avatar/department/branch on users,
-- password/refresh columns removed (Supabase Auth now owns credentials),
-- role list reconciled to Super Admin/Company Admin/Branch Manager/Staff/Driver/Customer,
-- broader permission set with Super Admin granted everything by default.

create table if not exists public.departments (
    id uuid primary key default gen_random_uuid(),
    name text not null unique,
    description text,
    branch_id uuid references public.branches(id) on delete set null,
    created_at timestamptz not null default now()
);

alter table public.users
    drop column if exists password_hash,
    drop column if exists refresh_token,
    drop column if exists refresh_token_expiry_time,
    add column if not exists avatar_url text,
    add column if not exists department_id uuid references public.departments(id) on delete set null,
    add column if not exists branch_id uuid references public.branches(id) on delete set null;

alter table public.users alter column id drop default;
alter table public.users alter column phone_number drop not null;

-- Reconcile roles to: Super Admin, Company Admin, Branch Manager, Staff, Driver, Customer
insert into public.roles (name, description, is_system)
values ('Company Admin', 'Company-wide administrator', true)
on conflict (name) do nothing;

update public.roles set name = 'Branch Manager', description = 'Manages a single branch''s operations'
where name = 'Branch Admin' and not exists (select 1 from public.roles where name = 'Branch Manager');

update public.roles set name = 'Staff', description = 'General operational staff'
where name = 'Fleet Manager' and not exists (select 1 from public.roles where name = 'Staff');

update public.users set role_id = (select id from public.roles where name = 'Staff')
where role_id = (select id from public.roles where name = 'Customer Support');

delete from public.roles where name = 'Customer Support';

-- Broader permission set
insert into public.permissions (name, description) values
    ('users.read', 'View users'),
    ('users.write', 'Create, update, and manage users'),
    ('roles.read', 'View roles'),
    ('roles.write', 'Create, update, and delete roles'),
    ('departments.read', 'View departments'),
    ('departments.write', 'Create, update, and delete departments'),
    ('invoices.read', 'View invoices'),
    ('invoices.write', 'Generate invoices'),
    ('insurance.read', 'View insurance records'),
    ('insurance.write', 'Manage insurance records'),
    ('customers.read', 'View customers'),
    ('customers.write', 'Create and manage customers')
on conflict (name) do nothing;

-- Super Admin gets every permission by default
insert into public.role_permissions (role_id, permission_id)
select r.id, p.id
from public.roles r
cross join public.permissions p
where r.name = 'Super Admin'
on conflict do nothing;
