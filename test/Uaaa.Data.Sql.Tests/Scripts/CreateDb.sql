-- create people table
create table People(
	Id int identity(1,1) not null primary key,
	Name nvarchar(50) not null,
	Surname nvarchar(50) not null,
	Age int null,
	CreateDateTimeUtc datetime not null,
	ChangedDateTimeUtc datetime null
);