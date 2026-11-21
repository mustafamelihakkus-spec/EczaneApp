using System.Data;

namespace EczaneApp
{
    // Ilac tablosuyla ilgili işler (F1, F7)
    public class IlacIslemleri
    {
        private readonly Veritabani vt;

        public IlacIslemleri(Veritabani vt)
        {
            this.vt = vt;
        }

        // F1 - Tüm ilaçlar
        public DataTable Listele()
        {
            return vt.Sorgula(
                "SELECT IlacID AS [No], IlacAdi AS [İlaç Adı], Fiyat AS [Fiyat (TL)], StokMiktari AS [Stok] " +
                "FROM Ilac ORDER BY IlacAdi");
        }

        // F7 - Stoğu 20'nin altında olanlar (S2 sorgusu)
        public DataTable DusukStok()
        {
            return vt.Sorgula(
                "SELECT IlacAdi AS [İlaç], StokMiktari AS [Stok] " +
                "FROM Ilac WHERE StokMiktari < 20 ORDER BY StokMiktari");
        }

        // Reçete ekranındaki açılır liste için
        public DataTable Secim()
        {
            return vt.Sorgula(
                "SELECT IlacID AS ID, IlacAdi + N'  (stok: ' + CAST(StokMiktari AS NVARCHAR(10)) + N')' AS Metin " +
                "FROM Ilac ORDER BY IlacAdi");
        }

        public int Sayi()
        {
            return Convert.ToInt32(vt.TekDeger("SELECT COUNT(*) FROM Ilac"));
        }
    }
}
