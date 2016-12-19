-- initialize database

-- create uaaa_db_version table
if not exists(select * from sys.tables where name = 'uaaa_db_version')
create table uaaa_db_version(
	version int not null constraint pk_uaaa_db_version_version primary key,
	created_date datetime not null constraint df_uaaa_db_version_created_date default getutcdate()
);
