/*
ECZANEDB — SQL SUNUM SORULARI

SSMS'de çalıştırma
1. SSMS'yi aç. Sunucu türü olarak Database Engine'i seç.
2. Sunucu adı alanına .\SQLEXPRESS yaz, Windows Authentication ile bağlan.
3. Object Explorer'da Databases > EczaneDB > Tables yolunu aç.
   Bir tablonun Columns ve Keys düğümlerinden sütunları ve anahtarları incele.
4. EczaneDB'ye sağ tıkla ve New Query'yi seç. Üstteki veritabanı
   açılır listesinin EczaneDB gösterdiğini kontrol et.
5. Bu dosyayı File > Open > File ile aç. Bir örneğin yalnızca sorgu
   satırlarını seçip Execute'a tıkla veya F5'e bas.
6. Sonucu Results, SQL Server iletilerini Messages sekmesinde incele.
   Seçim yapmadan F5'e basarsan dosyadaki bütün örnekler çalışır.

Doğrulanan canlı şema
PK (primary key) her satırı benzersiz tanımlar. FK (foreign key) başka
bir tablodaki anahtara başvurur. Aşağıdaki iki Telefon sütunu NULL olabilir.
Diğer bütün sütunlar NOT NULL. IDENTITY değerini SQL Server otomatik üretir.

dbo.Doktor
  DoktorID int (PK, IDENTITY), Ad nvarchar(50), Soyad nvarchar(50),
  Uzmanlik nvarchar(60). FK yok.
dbo.Eczane
  EczaneID int (PK, IDENTITY), EczaneAdi nvarchar(80), Adres nvarchar(150),
  Telefon nvarchar(20) NULL. FK yok.
dbo.Hasta
  HastaID int (PK, IDENTITY), TCKimlikNo char(11), Ad nvarchar(50),
  Soyad nvarchar(50), Telefon nvarchar(20) NULL. FK yok.
dbo.Ilac
  IlacID int (PK, IDENTITY), IlacAdi nvarchar(100), Fiyat decimal(10,2),
  StokMiktari int (varsayılan 0). FK yok.
dbo.Recete
  ReceteNo int (PK, IDENTITY değil), Tarih date (varsayılan sunucunun
  bugünkü tarihi), Adet int, DoktorID int, HastaID int, EczaneID int,
  IlacID int.
  FK DoktorID -> dbo.Doktor(DoktorID)
  FK HastaID  -> dbo.Hasta(HastaID)
  FK EczaneID -> dbo.Eczane(EczaneID)
  FK IlacID   -> dbo.Ilac(IlacID)
  Her reçete bu dört tablonun her birinde bir satıra başvurur.
  Aynı doktor, hasta, eczane veya ilaç birden fazla reçetede bulunabilir.

Örnekler hasta adı, T.C. kimlik numarası ve telefon numarası göstermez.
*/

USE [EczaneDB];
GO

-- KOLAY

-- 01. Doktorların uzmanlık alanları nelerdir?
-- Her doktorun uzmanlık alanını gösterir. Aynı uzmanlık tekrar edebilir.
SELECT Uzmanlik
FROM dbo.Doktor
ORDER BY Uzmanlik;

-- 02. Kayıtlı eczaneleri alfabetik olarak nasıl listeleriz?
-- Eczane adını ve tablo anahtarını getirir. Aynı adda anahtara göre sıralar.
SELECT EczaneID, EczaneAdi
FROM dbo.Eczane
ORDER BY EczaneAdi, EczaneID;

-- 03. Veritabanında kaç hasta var?
-- COUNT(*) tüm hasta satırlarını sayar ve tek sayı döndürür.
SELECT COUNT(*) AS HastaSayisi
FROM dbo.Hasta;

-- 04. İlaçların fiyat ve stok bilgilerini nasıl listeleriz?
-- İlaçları en yüksek fiyattan başlayarak gösterir.
SELECT IlacAdi, Fiyat, StokMiktari
FROM dbo.Ilac
ORDER BY Fiyat DESC, IlacID;

-- 05. En yeni 10 reçetenin tarihleri ve adetleri nelerdir?
-- TOP ilk 10 satırı alır. Tarih eşitliğinde büyük reçete numarası öne gelir.
SELECT TOP (10) Tarih, Adet
FROM dbo.Recete
ORDER BY Tarih DESC, ReceteNo DESC;

-- 06. Stoğu 20'nin altında kalan ilaçlar hangileridir?
-- WHERE stok eşiğini uygular. En düşük stok ilk sırada görünür.
SELECT IlacAdi, StokMiktari
FROM dbo.Ilac
WHERE StokMiktari < 20
ORDER BY StokMiktari, IlacAdi, IlacID;

-- 07. Adı A ile başlayan ilaçlar hangileridir?
-- LIKE içindeki % sıfır veya daha fazla karakteri temsil eder.
SELECT IlacAdi
FROM dbo.Ilac
WHERE IlacAdi LIKE N'A%'
ORDER BY IlacAdi, IlacID;

-- ORTA

-- 08. Toplam stok adedi ve ortalama ilaç fiyatı nedir?
-- SUM stokları toplar, AVG fiyatların aritmetik ortalamasını verir.
SELECT COALESCE(SUM(CONVERT(bigint, StokMiktari)), 0) AS ToplamStok,
       CAST(AVG(Fiyat) AS decimal(10, 2)) AS OrtalamaFiyat
FROM dbo.Ilac;

-- 09. Her uzmanlık alanında kaç doktor bulunuyor?
-- GROUP BY aynı uzmanlıktaki doktorları gruplar, COUNT(*) her grubu sayar.
SELECT Uzmanlik, COUNT(*) AS DoktorSayisi
FROM dbo.Doktor
GROUP BY Uzmanlik
ORDER BY DoktorSayisi DESC, Uzmanlik;

-- 10. En az iki reçete yazılmış günler hangileridir?
-- HAVING, gruplama sonrasında reçete sayısına göre günleri süzer.
SELECT Tarih, COUNT(*) AS ReceteSayisi
FROM dbo.Recete
GROUP BY Tarih
HAVING COUNT(*) >= 2
ORDER BY Tarih DESC;

-- 11. Reçetelerde yer alan ilaç adını nasıl gösteririz?
-- INNER JOIN, Recete.IlacID ile Ilac.IlacID eşleşmesini kullanır.
SELECT TOP (10) r.Tarih, i.IlacAdi, r.Adet
FROM dbo.Recete AS r
INNER JOIN dbo.Ilac AS i ON i.IlacID = r.IlacID
ORDER BY r.Tarih DESC, r.ReceteNo DESC;

-- 12. Her eczaneye kaç reçete bağlıdır?
-- LEFT JOIN tüm eczaneleri korur. COUNT(r.ReceteNo) eşleşme yoksa 0 verir.
SELECT e.EczaneID, e.EczaneAdi, COUNT(r.ReceteNo) AS ReceteSayisi
FROM dbo.Eczane AS e
LEFT JOIN dbo.Recete AS r ON r.EczaneID = e.EczaneID
GROUP BY e.EczaneID, e.EczaneAdi
ORDER BY ReceteSayisi DESC, e.EczaneID;

-- 13. İçinde bulunulan ayda kaç reçete var, toplam kaç adet ilaç yazıldı?
-- WHERE ayın başlangıcını dahil eder ve sonraki ayın başlangıcını hariç tutar.
SELECT COUNT(*) AS ReceteSayisi,
       COALESCE(SUM(CONVERT(bigint, Adet)), 0) AS ToplamAdet
FROM dbo.Recete
WHERE Tarih >= DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)
  AND Tarih < DATEADD(month, 1, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1));

-- 14. Fiyatı tüm ilaçların ortalamasından yüksek olan ilaçlar hangileri?
-- Alt sorgu ortalama fiyatı hesaplar. Dış sorgu her fiyatı onunla karşılaştırır.
SELECT IlacAdi, Fiyat
FROM dbo.Ilac
WHERE Fiyat > (SELECT AVG(Fiyat) FROM dbo.Ilac)
ORDER BY Fiyat DESC, IlacID;

-- ZOR

-- 15. Her ilaç için reçete sayısı ve yazılan toplam adet nedir?
-- Eşleşmeyen ilaçlar listede kalır. Stok ile reçetelerde yazılan adet ayrı değerlerdir.
SELECT i.IlacID, i.IlacAdi,
       COUNT(r.ReceteNo) AS ReceteSayisi,
       COALESCE(SUM(CONVERT(bigint, r.Adet)), 0) AS YazilanToplamAdet
FROM dbo.Ilac AS i
LEFT JOIN dbo.Recete AS r ON r.IlacID = i.IlacID
GROUP BY i.IlacID, i.IlacAdi
ORDER BY YazilanToplamAdet DESC, i.IlacID;

-- 16. Uzmanlık ve eczane bazında en az iki reçete içeren gruplar hangileri?
-- İki JOIN, reçeteyi doktor uzmanlığına ve eczaneye bağlar.
-- GROUP BY grupları oluşturur. HAVING az reçeteli grupları eler.
SELECT d.Uzmanlik, e.EczaneAdi, COUNT(*) AS ReceteSayisi
FROM dbo.Recete AS r
INNER JOIN dbo.Doktor AS d ON d.DoktorID = r.DoktorID
INNER JOIN dbo.Eczane AS e ON e.EczaneID = r.EczaneID
GROUP BY d.Uzmanlik, e.EczaneID, e.EczaneAdi
HAVING COUNT(*) >= 2
ORDER BY ReceteSayisi DESC, d.Uzmanlik, e.EczaneAdi, e.EczaneID;

-- 17. Reçetesi olan ve olmayan kaç hasta var?
-- İç sorgu HastaID başına bir satır üretir. LEFT JOIN tüm hastaları korur.
-- Böylece birden fazla reçetesi olan hasta yalnızca bir kez sayılır.
SELECT COUNT(r.HastaID) AS RecetesiOlanHasta,
       COUNT(*) - COUNT(r.HastaID) AS RecetesiOlmayanHasta
FROM dbo.Hasta AS h
LEFT JOIN (SELECT DISTINCT HastaID FROM dbo.Recete) AS r
       ON r.HastaID = h.HastaID;

-- 18. Eczane başına reçetelerin güncel fiyatlarla hesaplanan tutarı nedir?
-- SUM(adet * fiyat) tahmini tutarı verir. Ilac.Fiyat güncel fiyat olduğu
-- için sonuç geçmiş satış fiyatlarını temsil etmez.
SELECT e.EczaneID, e.EczaneAdi,
       COALESCE(SUM(r.Adet * i.Fiyat), 0) AS GuncelFiyatlaTutar
FROM dbo.Eczane AS e
LEFT JOIN dbo.Recete AS r ON r.EczaneID = e.EczaneID
LEFT JOIN dbo.Ilac AS i ON i.IlacID = r.IlacID
GROUP BY e.EczaneID, e.EczaneAdi
ORDER BY GuncelFiyatlaTutar DESC, e.EczaneID;

-- 19. Hiçbir reçetede yazılmamış ilaçlar hangileridir?
-- NOT EXISTS yalnızca eşleşen Recete satırı bulunmayan ilaçları seçer.
SELECT i.IlacAdi, i.StokMiktari
FROM dbo.Ilac AS i
WHERE NOT EXISTS (SELECT 1 FROM dbo.Recete AS r WHERE r.IlacID = i.IlacID)
ORDER BY i.IlacAdi, i.IlacID;

-- 20. Her yıl ve ay için reçete sayısı ve birikimli reçete sayısı nedir?
-- CTE ay bazında sayar. Pencere SUM'u ayları kronolojik sırayla biriktirir.
-- Reçete içermeyen aylar sonuçta görünmez.
;WITH Aylik AS
(
    SELECT DATEFROMPARTS(YEAR(Tarih), MONTH(Tarih), 1) AS AyBaslangici,
           COUNT_BIG(*) AS ReceteSayisi
    FROM dbo.Recete
    GROUP BY DATEFROMPARTS(YEAR(Tarih), MONTH(Tarih), 1)
)
SELECT AyBaslangici, ReceteSayisi,
       SUM(ReceteSayisi) OVER
       (ORDER BY AyBaslangici ROWS UNBOUNDED PRECEDING) AS BirikimliReceteSayisi
FROM Aylik
ORDER BY AyBaslangici;
