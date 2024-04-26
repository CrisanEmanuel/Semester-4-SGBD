
-- PHANTOM READS
begin transaction
		select * from Librarie
		waitfor delay '00:00:10'
		select * from Librarie
commit transaction

set transaction isolation level serializable -- PROBLEM: LEVEL REPEATABLE READ
begin transaction
		select * from Librarie
		waitfor delay '00:00:10'
		select * from Librarie
commit transaction