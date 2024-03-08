CREATE DATABASE WPF_Machine;
GO

USE WPF_Machine;
GO

CREATE TABLE Category (
    ID INT IDENTITY,
    CategoryName VARCHAR(255),
    Description VARCHAR(255),
    PRIMARY KEY (ID)
);

CREATE TABLE Product (
    ProductID INT IDENTITY,
    ProductName VARCHAR(255),
    Price FLOAT,
    Quantity INT,
    Description VARCHAR(255),
    [expiration date] DATE,
    Status BIT,
    ImageID INT,
    CategoryID INT,
    PRIMARY KEY (ProductID),
    FOREIGN KEY (CategoryID) REFERENCES Category (ID)
);

CREATE TABLE Payment (
    PaymentID INT PRIMARY KEY,
    Status BIT,
    Amount FLOAT,
    Method NVARCHAR(50)
);

CREATE TABLE [Order] (
    OrderId INT IDENTITY,
    PaymentID INT,
    Total FLOAT,
    Quantity INT,
    [Date Created] DATE,
    PRIMARY KEY (OrderId),
    FOREIGN KEY (PaymentID) REFERENCES Payment (PaymentID)
);

CREATE TABLE OrderDetail (
    ID INT IDENTITY,
    OrderId INT,
    ProductID INT,
    Price FLOAT,
    Status VARCHAR(255),
    CreationDate DATE,
    Quantity INT,
    Code VARCHAR(255),
    PRIMARY KEY (ID),
    FOREIGN KEY (OrderId) REFERENCES [Order] (OrderId),
    FOREIGN KEY (ProductID) REFERENCES Product (ProductID)
);

CREATE TABLE Image (
    ID INT IDENTITY,
    ProductID INT,
    ImagePath VARCHAR(255),
    PRIMARY KEY (ID),
    FOREIGN KEY (ProductID) REFERENCES Product (ProductID)
);
