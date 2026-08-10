-- SaaS Module: an additive "platform admin" layer for managing car rental
-- companies as customers of this software (tenant registration, subscription
-- plans, billing, plan limits, feature toggles, white-label branding, and
-- custom domain records). Shared database, tenant_id column isolation model.
-- Nothing in the existing schema is modified - none of the pre-existing
-- tables are touched by this script.

create table if not exists public.tenants (
    id uuid primary key default gen_random_uuid(),
    company_name text not null,
    slug text not null unique,
    contact_email text not null,
    contact_phone text,
    status text not null default 'Trial',
    trial_ends_at timestamptz,
    created_at timestamptz not null default now()
);

create table if not exists public.subscription_plans (
    id uuid primary key default gen_random_uuid(),
    name text not null,
    description text,
    monthly_price numeric(18,2) not null default 0,
    annual_price numeric(18,2) not null default 0,
    currency text not null default 'USD',
    is_active boolean not null default true,
    created_at timestamptz not null default now()
);

create table if not exists public.plan_limits (
    id uuid primary key default gen_random_uuid(),
    plan_id uuid not null references public.subscription_plans(id) on delete cascade,
    limit_key text not null,
    limit_value int not null default 0
);

create table if not exists public.plan_features (
    id uuid primary key default gen_random_uuid(),
    plan_id uuid not null references public.subscription_plans(id) on delete cascade,
    feature_key text not null,
    is_enabled boolean not null default false
);

create table if not exists public.tenant_feature_overrides (
    id uuid primary key default gen_random_uuid(),
    tenant_id uuid not null references public.tenants(id) on delete cascade,
    feature_key text not null,
    is_enabled boolean not null default false
);

create table if not exists public.subscriptions (
    id uuid primary key default gen_random_uuid(),
    tenant_id uuid not null references public.tenants(id) on delete cascade,
    plan_id uuid not null references public.subscription_plans(id) on delete restrict,
    status text not null default 'Trialing',
    billing_cycle text not null default 'Monthly',
    current_period_start timestamptz not null default now(),
    current_period_end timestamptz not null default (now() + interval '1 month'),
    cancel_at_period_end boolean not null default false,
    created_at timestamptz not null default now()
);

create table if not exists public.subscription_invoices (
    id uuid primary key default gen_random_uuid(),
    tenant_id uuid not null references public.tenants(id) on delete cascade,
    subscription_id uuid not null references public.subscriptions(id) on delete restrict,
    invoice_number text not null,
    period_start timestamptz not null,
    period_end timestamptz not null,
    amount numeric(18,2) not null default 0,
    currency text not null default 'USD',
    status text not null default 'Pending',
    paid_at timestamptz,
    gateway_reference text,
    created_at timestamptz not null default now()
);

create table if not exists public.tenant_usage_metrics (
    id uuid primary key default gen_random_uuid(),
    tenant_id uuid not null references public.tenants(id) on delete cascade,
    metric_key text not null,
    metric_value numeric(18,2) not null default 0,
    recorded_at timestamptz not null default now()
);

create table if not exists public.tenant_brandings (
    id uuid primary key default gen_random_uuid(),
    tenant_id uuid not null unique references public.tenants(id) on delete cascade,
    logo_url text,
    primary_color text,
    secondary_color text,
    company_display_name text,
    favicon_url text,
    updated_at timestamptz
);

create table if not exists public.tenant_domains (
    id uuid primary key default gen_random_uuid(),
    tenant_id uuid not null references public.tenants(id) on delete cascade,
    domain text not null,
    status text not null default 'Pending',
    created_at timestamptz not null default now()
);

create index if not exists idx_subscriptions_tenant_id on public.subscriptions(tenant_id);
create index if not exists idx_subscription_invoices_tenant_id on public.subscription_invoices(tenant_id);
create index if not exists idx_subscription_invoices_subscription_id on public.subscription_invoices(subscription_id);
create index if not exists idx_tenant_usage_metrics_tenant_id on public.tenant_usage_metrics(tenant_id);
create index if not exists idx_tenant_domains_tenant_id on public.tenant_domains(tenant_id);

-- New access boundary: platform staff who manage tenants/plans/billing are
-- distinct from a tenant's own "Super Admin"/"Company Admin" roles, which
-- only ever manage that one tenant's car rental business.
insert into public.roles (id, name, description, is_system)
values (gen_random_uuid(), 'Platform Owner', 'Manages SaaS tenants, plans, and billing for the platform itself', true)
on conflict (name) do nothing;

-- Starter plans so the module isn't empty on first load.
do $$
declare
    starter_id uuid;
    growth_id uuid;
    enterprise_id uuid;
begin
    insert into public.subscription_plans (id, name, description, monthly_price, annual_price, currency, is_active)
    values (gen_random_uuid(), 'Starter', 'Single branch, small fleet', 49, 490, 'USD', true)
    returning id into starter_id;

    insert into public.subscription_plans (id, name, description, monthly_price, annual_price, currency, is_active)
    values (gen_random_uuid(), 'Growth', 'Multiple branches, growing fleet', 149, 1490, 'USD', true)
    returning id into growth_id;

    insert into public.subscription_plans (id, name, description, monthly_price, annual_price, currency, is_active)
    values (gen_random_uuid(), 'Enterprise', 'Unlimited branches and fleet, priority support', 399, 3990, 'USD', true)
    returning id into enterprise_id;

    insert into public.plan_limits (plan_id, limit_key, limit_value) values
        (starter_id, 'MaxVehicles', 25), (starter_id, 'MaxBranches', 1), (starter_id, 'MaxUsers', 5),
        (growth_id, 'MaxVehicles', 150), (growth_id, 'MaxBranches', 5), (growth_id, 'MaxUsers', 25),
        (enterprise_id, 'MaxVehicles', -1), (enterprise_id, 'MaxBranches', -1), (enterprise_id, 'MaxUsers', -1);

    insert into public.plan_features (plan_id, feature_key, is_enabled) values
        (starter_id, 'AdvancedReports', false), (starter_id, 'WhiteLabel', false), (starter_id, 'CustomDomain', false), (starter_id, 'ApiAccess', false),
        (growth_id, 'AdvancedReports', true), (growth_id, 'WhiteLabel', false), (growth_id, 'CustomDomain', false), (growth_id, 'ApiAccess', true),
        (enterprise_id, 'AdvancedReports', true), (enterprise_id, 'WhiteLabel', true), (enterprise_id, 'CustomDomain', true), (enterprise_id, 'ApiAccess', true);
end $$;
