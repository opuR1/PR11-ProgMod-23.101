CREATE TABLE Suppliers (
    SupplierID INT IDENTITY(1,1) PRIMARY KEY,
    SupplyName NVARCHAR(255) NOT NULL,
    SupplyAddress NVARCHAR(500) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(100)
);
CREATE TABLE Stores (
    StoreID INT IDENTITY(1,1) PRIMARY KEY,
    StoreName NVARCHAR(255) NOT NULL,
    StoreAddress NVARCHAR(500) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(100)
);
create table Units (
	UnitID int primary key identity(1,1),
	UnitName NVARCHAR(50) NOT NULL
);
CREATE TABLE Materials (
    MaterialID INT IDENTITY(1,1) PRIMARY KEY,
    MaterialName NVARCHAR(255) NOT NULL,
    UnitID int foreign key references Units(UnitID) NOT NULL,
    MinStock DECIMAL(10,2),
    CurrentStock DECIMAL(10,2)
);
create table Seasons(
	SeasonID int primary key identity(1,1),
	SeasonName NVARCHAR(50) NOT NULL
);
CREATE TABLE ShoeModels (
    ModelID INT IDENTITY(1,1) PRIMARY KEY,
    ShoeModelName NVARCHAR(255) NOT NULL,
    Article NVARCHAR(50),
    SeasonID int foreign key references Seasons(SeasonID) NOT NULL,
    Price DECIMAL(10,2),
    CostPrice DECIMAL(10,2),
    ModelDescription NVARCHAR(1000)
);
create table Positions (
	PositionID int primary key identity(1,1),
	PositionName NVARCHAR(100) NOT NULL
);
CREATE TABLE Employees (
    EmployeeID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
	LastName NVARCHAR(50) NOT NULL,
	SurName NVARCHAR(50),
    PositionID int foreign key references Positions(PositionID) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(100) NOT NULL,
	UserID int foreign key references Users(UserID) NOT NULL
);
create table Statys(
	StatysID int primary key identity(1,1),
	StatysName NVARCHAR(50) NOT NULL
);
create table EquipTypes (
	TypeID int primary key identity(1,1),
	TypeName NVARCHAR(100) NOT NULL
);
create table Equipments(
	EquipID int primary key identity(1,1),
	EquipName NVARCHAR(100) not null,
	EquipTypeID int foreign key references EquipTypes(TypeID) NOT NULL,
	StatysID int foreign key references Statys(StatysID) NOT NULL
);
create table TMRequests (
	RequestID int primary key identity(1,1),
	EquipID int foreign key references Equipments(EquipID) NOT NULL,
	EmployeeID int foreign key references Employees(EmployeeID) NOT NULL,
	RequestDate DATE NOT NULL,
	TMRequestDescription NVARCHAR(1000),
	StatysID int foreign key references Statys(StatysID) NOT NULL
);
create table ReceiptInvoices (
	ReceiptID int primary key identity(1,1),
	ReceiptNumber NVARCHAR(50) NOT NULL,
	ReceiptDate DATE not null,
	SupplierID int foreign key references Suppliers(SupplierID) NOT NULL,
	TotalSum DECIMAL(10,2),
	StatysID int foreign key references Statys(StatysID) NOT NULL
);
create table Comings (
	ComingID int primary key identity(1,1),
	ReceiptID int foreign key references ReceiptInvoices(ReceiptID) NOT NULL,
	MaterialID int foreign key references Materials(MaterialID) NOT NULL,
	MaterialCount DECIMAL(10,2),
	MaterialPricePerUnit DECIMAL(10,2)
);
create table ExpenseInvoices (
	ExpenseID int primary key identity(1,1),
	ExpenseNumber NVARCHAR(50) NOT NULL,
	ExpenseDate DATE not null,
	StoreID int foreign key references Stores(StoreID) NOT NULL,
	TotalSum DECIMAL(10,2),
	StatysID int foreign key references Statys(StatysID) NOT NULL
);
Create table Departs (
	DepartID int primary key identity(1,1),
	ExpenseID int foreign key references ExpenseInvoices(ExpenseID) NOT NULL,
	ModelID int foreign key references ShoeModels(ModelID) NOT NULL,
	ShoesCount INT
);
create table Defects(
	DefectID int primary key identity(1,1),
	DefectDate DATE NOT NULL,
	ModelID int foreign key references ShoeModels(ModelID) NOT NULL,
	DefectCount INT,
	Cause NVARCHAR(500),
	StatysID int foreign key references Statys(StatysID) NOT NULL
);
create table Reviews (
	ReviewID int primary key identity(1,1),
	ReviewDate DATE NOT NULL,
	StoreID int foreign key references Stores(StoreID) NOT NULL,
	ModelID int foreign key references ShoeModels(ModelID) NOT NULL,
	ReviewDesc NVARCHAR(1000),
	Rating int NOT NULL CHECK (Rating >= 1 AND Rating <= 5)
);
create table Clients (
	ClientID int primary key identity(1,1),
	FirstName NVARCHAR(50) NOT NULL,
	LastName NVARCHAR(50) NOT NULL,
	SurName NVARCHAR(50),
	Email NVARCHAR(50) NOT NULL,
	UserID int foreign key references Users(UserID) NOT NULL
);
create table Roles (
	RoleID int primary key identity(1,1),
	Role NVARCHAR(100)
);
create table Users (
	UserID int primary key identity(1,1),
	Email NVARCHAR(50) NOT NULL,
	Password NVARCHAR(100) NOT NULL,
	RoleID int foreign key references Roles(RoleID) NOT NULL
);
