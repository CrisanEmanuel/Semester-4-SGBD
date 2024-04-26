
-- DIRTY READS 
begin transaction
	update Librarie set nume = 'DIRTY READ'
	where id_librarie = 1 or id_librarie = 2;
	waitfor delay '00:00:10'
rollback transaction
