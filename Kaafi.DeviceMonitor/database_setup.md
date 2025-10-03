# Database Setup Scripts

## Create Devices Table

CREATE TABLE att_Devices (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name VARCHAR(255) NOT NULL,
    IP VARCHAR(50) NOT NULL,
    Status VARCHAR(50) NOT NULL,
    LastActive DATETIME NOT NULL
);

## Create DeviceHistories Table

CREATE TABLE att_DeviceHistories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    DeviceId INT FOREIGN KEY REFERENCES att_Devices(Id),
    Status VARCHAR(50) NOT NULL,
    Timestamp DATETIME NOT NULL
);

## Stored Procedure to Insert Device

CREATE PROCEDURE att_InsertDevice
    @Name VARCHAR(255),
    @IP VARCHAR(50),
    @Status VARCHAR(50)
AS
BEGIN
    INSERT INTO att_Devices (Name, IP, Status, LastActive)
    VALUES (@Name, @IP, @Status, GETDATE());
END;

## Stored Procedure to Update Device

CREATE PROCEDURE att_UpdateDevice
    @Id INT,
    @Name VARCHAR(255),
    @IP VARCHAR(50),
    @Status VARCHAR(50)
AS
BEGIN
    UPDATE att_Devices
    SET Name = @Name, IP = @IP, Status = @Status, LastActive = GETDATE()
    WHERE Id = @Id;
END;

## Stored Procedure to Delete Device

CREATE PROCEDURE att_DeleteDevice
    @Id INT
AS
BEGIN
    DELETE FROM att_Devices WHERE Id = @Id;
END;

## View for Active Devices

CREATE VIEW att_ActiveDevices AS
SELECT * FROM att_Devices WHERE Status = 'Online';


## Trigger to Log Device Status Change

CREATE TRIGGER att_LogDeviceStatusChange
ON att_Devices
AFTER UPDATE
AS
BEGIN
    INSERT INTO att_DeviceHistories (DeviceId, Status, Timestamp)
    SELECT Id, Status, GETDATE() FROM inserted;
END;

## Stored Procedures

### Insert Device

CREATE PROCEDURE att_InsertDevice
    @Name VARCHAR(255),
    @IP VARCHAR(50),
    @Status VARCHAR(50)
AS
BEGIN
    INSERT INTO att_Devices (Name, IP, Status, LastActive)
    VALUES (@Name, @IP, @Status, GETDATE());
END;


### Update Device

CREATE PROCEDURE att_UpdateDevice
    @Id INT,
    @Name VARCHAR(255),
    @IP VARCHAR(50),
    @Status VARCHAR(50)
AS
BEGIN
    UPDATE att_Devices
    SET Name = @Name, IP = @IP, Status = @Status, LastActive = GETDATE()
    WHERE Id = @Id;
END;


### Delete Device

CREATE PROCEDURE att_DeleteDevice
    @Id INT
AS
BEGIN
    DELETE FROM att_Devices WHERE Id = @Id;
END;

## View for Active Devices

CREATE VIEW att_ActiveDevices AS
SELECT * FROM att_Devices WHERE Status = 'Online';


## View for Device History

CREATE VIEW att_DeviceHistory AS
SELECT d.Name, dh.Status, dh.Timestamp
FROM att_DeviceHistories dh
JOIN att_Devices d ON dh.DeviceId = d.Id
ORDER BY dh.Timestamp DESC;
