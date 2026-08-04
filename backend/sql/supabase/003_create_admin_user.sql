-- Creates (or resets the password of) a Super Admin login.
-- Run in the Supabase SQL editor, or via psql against the app's Postgres database.

insert into public.users (
    first_name, last_name, email, phone_number,
    password_hash, is_email_verified, is_active, role_id
)
select
    'Admin', 'User', 'admin@carrent.local', null,
    '$2a$11$JdYKJoo61Aw/jiu6zfkjoeQqqHfM/OaCdgmTBiYMo2MSKxbLL4Ti.',
    true, true, r.id
from public.roles r
where r.name = 'Super Admin'
on conflict (email) do update
    set password_hash = excluded.password_hash,
        role_id = excluded.role_id,
        is_active = true;
