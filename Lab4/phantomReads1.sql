
-- PHANTOM READS
begin transaction
	waitfor delay '00:00:10'
	insert into Librarie(nume, adresa) values
		('PHANTOM', 'PHANTOM')
commit transaction

select * from Librarie
delete from Librarie where nume like 'phantom%'