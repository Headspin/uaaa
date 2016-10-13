-- create People table
if exists(select * from sys.tables where name = 'People')
	drop table People;
create table People(
	Id int identity(1,1) not null primary key,
	Name nvarchar(50) not null,
	Surname nvarchar(50) not null,
	Age int null,
	CreatedDateTimeUtc datetime not null default(GETUTCDATE()),
	ChangedDateTimeUtc datetime null
);