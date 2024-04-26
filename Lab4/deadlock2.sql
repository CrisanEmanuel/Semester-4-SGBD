
-- DEADLOCK
set deadlock_priority high
set deadlock_priority low

begin transaction
	update Domeniu set descriere = 'T2' where id_domeniu = 1;
	select * from Domeniu where id_domeniu = 1
	waitfor delay '00:00:10'
	update Librarie set nume = 'T2' where id_librarie = 1;
	select * from Librarie where id_librarie = 1
commit transaction