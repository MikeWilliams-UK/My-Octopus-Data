CREATE TABLE Properties (Id INTEGER);
CREATE UNIQUE INDEX Idx_Properties ON Properties (Id ASC);

CREATE TABLE MeterPoints (Mpxn STRING PRIMARY KEY UNIQUE NOT NULL, FuelType STRING NOT NULL, ProfileClass INTEGER, ConsumptionStandard INTEGER);
CREATE UNIQUE INDEX Idx_MeterPoints ON MeterPoints (Mpxn ASC);

CREATE TABLE Meters (SerialNumber STRING PRIMARY KEY UNIQUE NOT NULL, FuelType STRING NOT NULL);
CREATE UNIQUE INDEX Idx_Meters ON Meters (SerialNumber ASC);

-- NB: Only Electric meters have registers (in my data)
CREATE TABLE MeterRegisters (Id STRING PRIMARY KEY NOT NULL, FuelType STRING NOT NULL, Rate STRING);
CREATE UNIQUE INDEX Idx_MeterRegisters ON MeterRegisters (Id ASC);

CREATE TABLE Agreements (StartDate STRING NOT NULL, EndDate STRING, FuelType STRING NOT NULL, TariffCode STRING NOT NULL);
CREATE UNIQUE INDEX Idx_Agreements ON Agreements (StartDate ASC, TariffCode ASC);

CREATE TABLE HalfHourlyElectric (StartTime STRING PRIMARY KEY UNIQUE NOT NULL, Consumption DOUBLE);
CREATE INDEX Idx_HalfHourlyElectric ON HalfHourlyElectric (StartTime ASC);

CREATE TABLE HalfHourlyGas (StartTime STRING PRIMARY KEY UNIQUE NOT NULL, Consumption DOUBLE);
CREATE INDEX Idx_HalfHourlyGas ON HalfHourlyGas (StartTime ASC);