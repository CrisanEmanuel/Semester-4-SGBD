
-- DIRTY READS 
set transaction isolation level read uncommitted
begin transaction
	select * from Librarie
	waitfor delay '00:00:10'
	select * from Librarie
commit transaction


set transaction isolation level read committed -- Problem: UNCOMMITTED
	begin tran
	select * from Librarie
	waitfor delay '00:00:10'
	select * from Librarie
commit tran

