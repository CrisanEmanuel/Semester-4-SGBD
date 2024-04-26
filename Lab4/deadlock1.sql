
-- DEADLOCK
set deadlock_priority high
set deadlock_priority low

begin transaction
	update Librarie set nume = 'T1' where id_librarie = 1;
	select * from Librarie where id_librarie = 1
	waitfor delay '00:00:10'
	update Domeniu set descriere = 'T1' where id_domeniu = 1;
	select * from Domeniu where id_domeniu = 1
commit transaction

update Librarie set nume = 'nume1' where id_librarie = 1;
update Domeniu set descriere = 'domeniu1' where id_domeniu = 1;
select * from Librarie
select * from Domeniu