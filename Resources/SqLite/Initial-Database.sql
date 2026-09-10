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

CREATE TABLE HalfHourlyConsumptionElectric (StartTime STRING PRIMARY KEY UNIQUE NOT NULL, Consumption DOUBLE);
CREATE INDEX Idx_HalfHourlyConsumptionElectric ON HalfHourlyConsumptionElectric (StartTime ASC);

CREATE TABLE HalfHourlyConsumptionGas (StartTime STRING PRIMARY KEY UNIQUE NOT NULL, Consumption DOUBLE);
CREATE INDEX Idx_HalfHourlyConsumptionGas ON HalfHourlyConsumptionGas (StartTime ASC);

CREATE TABLE HalfHourlyCostsElectric (StartTime STRING NOT NULL, Consumption DOUBLE, CostType STRING NOT NULL, RateExcVat DOUBLE, CostExcVat DOUBLE, RateIncVat DOUBLE, CostIncVat DOUBLE);
CREATE UNIQUE INDEX Idx_HalfHourlyCostsElectric ON HalfHourlyCostsElectric (StartTime ASC, CostType ASC);

CREATE TABLE HalfHourlyCostsGas (StartTime STRING NOT NULL, Consumption DOUBLE, CostType STRING NOT NULL, RateExcVat DOUBLE, CostExcVat DOUBLE, RateIncVat DOUBLE, CostIncVat DOUBLE);
CREATE UNIQUE INDEX Idx_HalfHourlyCostsGas ON HalfHourlyCostsGas (StartTime ASC, CostType ASC);

-- NB: The Charge Events don't have any cost data !!!

CREATE TABLE Chargers (Id TEXT PRIMARY KEY NOT NULL UNIQUE, Name TEXT NOT NULL, Status TEXT NOT NULL, LastActive TEXT);
CREATE TABLE ChargeEvents (ChargerId TEXT NOT NULL, StartTime TEXT NOT NULL, EndTime NOT NULL, EnergyAdded DOUBLE NOT NULL, TypeOfCharge TEXT NOT NULL, Problems TEXT);
CREATE UNIQUE INDEX Idx_ChargeEvents ON ChargeEvents (ChargerId ASC, StartTime ASC);
