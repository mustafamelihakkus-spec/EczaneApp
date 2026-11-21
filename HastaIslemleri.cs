using System.Data;
using Microsoft.Data.SqlClient;

namespace EczaneApp
{
    // Hasta tablosuyla ilgili işler (F2, F3, F6)
    public class HastaIslemleri
    {
        private readonly Veritabani vt;

        public HastaIslemleri(Veritabani vt)
        {
            this.vt = vt;
        }

        // F2 - Yeni hasta ekle (parametreli -> SQL Injection olmaz)
        public void Ekle(string tc, string ad, string soyad, string telefon)
        {
            vt.Calistir(
                "INSERT INTO Hasta (TCKimlikNo, Ad, Soyad, Telefon) VALUES (@tc, @ad, @soyad, @tel)",
                new SqlParameter("@tc", tc),
                new SqlParameter("@ad", ad),
                new SqlParameter("@soyad", soyad),
                new SqlParameter("@tel", string.IsNullOrWhiteSpace(telefon) ? (object)DBNull.Value : telefon));
        }

        // F3 - Hastaları listele
        public DataTable Listele()
        {
            return vt.Sorgula(
                "SELECT HastaID AS [No], TCKimlikNo AS [T.C. Kimlik No], Ad, Soyad, Telefon " +
                "FROM Hasta ORDER BY HastaID");
        }

        // F6 - T.C. ile hastanın reçeteleri (S7 sorgusu)
        public DataTable Receteleri(string tc)
        {
            return vt.Sorgula(
                "SELECT r.ReceteNo AS [Reçete No], r.Tarih, i.IlacAdi AS [İlaç], r.Adet " +
                "FROM Recete r " +
                "JOIN Hasta h ON h.HastaID = r.HastaID " +
                "JOIN Ilac i ON i.IlacID = r.IlacID " +
                "WHERE h.TCKimlikNo = @tc ORDER BY r.Tarih",
                new SqlParameter("@tc", tc));
        }

        // Reçete ekranındaki açılır liste için
        public DataTable Secim()
        {
            return vt.Sorgula(
                "SELECT HastaID AS ID, Ad + ' ' + Soyad + ' - ' + TCKimlikNo AS Metin " +
                "FROM Hasta ORDER BY Ad, Soyad");
        }

        public int Sayi()
        {
            return Convert.ToInt32(vt.TekDeger("SELECT COUNT(*) FROM Hasta"));
        }
    }
}
