
-- NON-REPEATABLE READS
insert into Librarie(nume, adresa) values ('OLD', 'OLD')

begin transaction
	waitfor delay '00:00:10';
	update Librarie set nume = 'NEW' where nume = 'OLD';
commit transaction

delete from Librarie where nume = 'NEW' or nume = 'OLD'
select * from Librarie