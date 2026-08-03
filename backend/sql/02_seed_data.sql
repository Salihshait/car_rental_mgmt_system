USE CarRentDb;
GO

INSERT INTO Roles (Id, Name, Description, IsSystem) VALUES
    (NEWID(), 'Super Admin', 'Platform administrator with full access', 1),
    (NEWID(), 'Branch Admin', 'Branch-level administrator', 1),
    (NEWID(), 'Fleet Manager', 'Operational fleet oversight', 1),
    (NEWID(), 'Customer Support', 'Support and customer care', 1),
    (NEWID(), 'Driver', 'Assigned vehicle driver', 1),
    (NEWID(), 'Customer', 'End customer account', 1);
GO
