-- create People table
if exists(select * from sys.tables where name = 'People')
	drop table People;