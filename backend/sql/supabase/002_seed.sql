insert into public.roles (id, name, description, is_system)
values
    (gen_random_uuid(), 'Super Admin', 'Platform administrator with full access', true),
    (gen_random_uuid(), 'Branch Admin', 'Branch-level administrator', true),
    (gen_random_uuid(), 'Fleet Manager', 'Operational fleet oversight', true),
    (gen_random_uuid(), 'Customer Support', 'Support and customer care', true),
    (gen_random_uuid(), 'Driver', 'Assigned vehicle driver', true),
    (gen_random_uuid(), 'Customer', 'End customer account', true)
on conflict (name) do nothing;

insert into public.branches (id, name, city, state, country, address, is_active)
values
    (gen_random_uuid(), 'Central Branch', 'New Delhi', 'Delhi', 'India', 'Connaught Place, New Delhi', true),
    (gen_random_uuid(), 'Airport Branch', 'Mumbai', 'Maharashtra', 'India', 'Mumbai Airport Road', true)
on conflict (name) do nothing;

insert into public.permissions (id, name, description)
values
    (gen_random_uuid(), 'vehicles.read', 'Read vehicle data'),
    (gen_random_uuid(), 'vehicles.write', 'Manage vehicle data'),
    (gen_random_uuid(), 'bookings.read', 'Read bookings'),
    (gen_random_uuid(), 'bookings.write', 'Manage bookings'),
    (gen_random_uuid(), 'payments.read', 'Read payments'),
    (gen_random_uuid(), 'payments.write', 'Manage payments')
on conflict (name) do nothing;
