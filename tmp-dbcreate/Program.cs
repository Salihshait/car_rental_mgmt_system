using System;
using Npgsql;

var connectionString = "Host=db.zjudixjgsyeglevlzrgp.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=sKiYzAt87hdxpOrO;SSL Mode=Require;";
var email = "customer@carrent.local";
var password = "Password123!";
var hash = BCrypt.Net.BCrypt.HashPassword(password);

await using var conn = new NpgsqlConnection(connectionString);
await conn.OpenAsync();

var roleCmd = new NpgsqlCommand("select id from public.roles where name = 'Customer' limit 1", conn);
var roleIdResult = await roleCmd.ExecuteScalarAsync();
if (roleIdResult is null || roleIdResult is DBNull)
{
    throw new Exception("Customer role not found in database.");
}

var roleId = (Guid)roleIdResult;

await using var insertUser = new NpgsqlCommand(
    @"insert into public.users (id, first_name, last_name, email, phone_number, password_hash, is_email_verified, is_active, role_id, created_at)
    values (gen_random_uuid(), 'Test', 'Customer', @email, '9999999999', @hash, true, true, @roleId, now())
    on conflict (email) do nothing;",
    conn);

insertUser.Parameters.AddWithValue("email", email);
insertUser.Parameters.AddWithValue("hash", hash);
insertUser.Parameters.AddWithValue("roleId", roleId);
await insertUser.ExecuteNonQueryAsync();

await using var verifyCmd = new NpgsqlCommand("select email, password_hash from public.users where email = @email", conn);
verifyCmd.Parameters.AddWithValue("email", email);
await using var verifyReader = await verifyCmd.ExecuteReaderAsync();
if (await verifyReader.ReadAsync())
{
    Console.WriteLine($"Created login credential: {verifyReader.GetString(0)} / {password}");
    Console.WriteLine($"Password hash stored: {verifyReader.GetString(1)}");
}
else
{
    Console.WriteLine("Credential creation could not be verified.");
}
