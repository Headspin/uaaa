-- create user_profiles table
if not exists(select * from sys.tables where name = 'uaaa_sql_test_table')
create table uaaa_sql_test_table(
	id int identity(1,1) constraint pk_uaaa_sql_test_table_id primary key,
	created_date datetime not null constraint df_uaaa_sql_test_table_created_date default getutcdate(),
	value nvarchar(50) not null
);