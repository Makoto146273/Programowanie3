using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Programowanie3
{
    public partial class Task3 : Window
    {
        private GraczViewModel gracz;
        private bool panelUkryty = false;

        public Task3()
        {
            InitializeComponent();
            LogikaGry gra = new LogikaGry();
            GraczViewModel gracz1 = new GraczViewModel(gra, 0);
            GraczViewModel gracz2 = new GraczViewModel(gra, 1);
            UstawGracza(gracz1);
            Task3 drugieOkno = new Task3(gracz2);
            drugieOkno.Show();
        }

        private Task3(GraczViewModel gracz)
        {
            InitializeComponent();
            UstawGracza(gracz);
        }

        private void UstawGracza(GraczViewModel gracz)
        {
            this.gracz = gracz;
            DataContext = gracz;
            for (int i = 0; i < 100; i++)
            {
                Button przycisk = new Button
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(0.5),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Width = 40,
                    Height = 40,
                    Padding = new Thickness(0),
                    Margin = new Thickness(1)
                };
                przycisk.DataContext = gracz.Pola[i];
                przycisk.SetBinding(ContentControl.ContentProperty, "Tekst");
                przycisk.SetBinding(IsEnabledProperty, "CzyAktywne");
                przycisk.SetBinding(BackgroundProperty, "Tlo");
                przycisk.SetBinding(ForegroundProperty, "KolorTekstu");
                przycisk.Click += Pole_Click;
                Plansza.Children.Add(przycisk);
            }
            Loaded += OknoZaladowane;
            Closed += OknoZamkniete;
            PanelUstawiania.IsVisibleChanged += PanelWidocznoscZmieniona;
        }

        private void Pole_Click(object nadawca, RoutedEventArgs e)
        {
            Button przycisk = (Button)nadawca;
            PoleViewModel pole = (PoleViewModel)przycisk.DataContext;
            gracz.KliknijPole(pole.Wiersz, pole.Kolumna);
        }

        private void WybierzStatek_Click(object nadawca, RoutedEventArgs e)
        {
            Button przycisk = (Button)nadawca;
            PrzyciskStatkuViewModel model = (PrzyciskStatkuViewModel)przycisk.DataContext;
            gracz.WybierzStatek(model.Dlugosc);
        }

        private void Obroc_Click(object nadawca, RoutedEventArgs e)
        {
            gracz.Obroc();
        }

        private void Gumka_Click(object nadawca, RoutedEventArgs e)
        {
            gracz.PrzelaczGumke();
        }

        private void Potwierdz_Click(object nadawca, RoutedEventArgs e)
        {
            gracz.Potwierdz();
        }

        private void OknoZaladowane(object nadawca, RoutedEventArgs e)
        {
            ZablokujRozmiar();
        }

        private void OknoZamkniete(object nadawca, EventArgs e)
        {
            gracz.Odlacz();
        }

        private void PanelWidocznoscZmieniona(object nadawca, DependencyPropertyChangedEventArgs e)
        {
            if (!panelUkryty && PanelUstawiania.Visibility == Visibility.Collapsed)
            {
                panelUkryty = true;
                MinWidth = 0;
                MinHeight = 0;
                SizeToContent = SizeToContent.WidthAndHeight;
                ZablokujRozmiar();
            }
        }

        private void ZablokujRozmiar()
        {
            UpdateLayout();
            MinWidth = ActualWidth;
            MinHeight = ActualHeight;
            SizeToContent = SizeToContent.Manual;
        }
    }

    public class LogikaGry
    {
        private readonly Pole[,,] plansze = new Pole[2, 10, 10];
        private readonly int[] flota = { 0, 4, 3, 2, 1 };
        private readonly int[,] ustawioneStatki = new int[2, 5];
        private readonly int[] ustawioneMaszty = new int[2];
        private readonly bool[] potwierdzone = new bool[2];
        private readonly int[] trafienia = new int[2];
        private readonly int liczbaMasztow;
        private int aktualnaTura = 0;
        private int zwyciezca = -1;
        private bool graRozpoczeta = false;
        private bool graZakonczona = false;
        public event Action Zmieniono;

        public LogikaGry()
        {
            for (int dlugosc = 1; dlugosc < flota.Length; dlugosc++)
            {
                liczbaMasztow += dlugosc * flota[dlugosc];
            }
            for (int g = 0; g < 2; g++)
            {
                for (int w = 0; w < 10; w++)
                {
                    for (int k = 0; k < 10; k++)
                    {
                        plansze[g, w, k] = new Pole();
                    }
                }
            }
        }

        public Pole PobierzWidocznePole(int gracz, int wiersz, int kolumna)
        {
            if (!graRozpoczeta)
                return plansze[gracz, wiersz, kolumna];
            return plansze[1 - gracz, wiersz, kolumna];
        }

        public int PobierzPozostaleStatki(int gracz, int dlugosc)
        {
            return flota[dlugosc] - ustawioneStatki[gracz, dlugosc];
        }

        public bool CzyWszystkieStatkiUstawione(int gracz)
        {
            return ustawioneMaszty[gracz] == liczbaMasztow;
        }

        public void UstawStatek(int gracz, int wiersz, int kolumna, int dlugosc, bool poziomo)
        {
            if (graRozpoczeta || potwierdzone[gracz] || dlugosc < 1 || dlugosc > 4 || PobierzPozostaleStatki(gracz, dlugosc) <= 0)
                return;
            if ((poziomo && kolumna + dlugosc > 10) || (!poziomo && wiersz + dlugosc > 10))
                return;
            int krokWiersza = 1;
            int krokKolumny = 0;
            if (poziomo)
            {
                krokWiersza = 0;
                krokKolumny = 1;
            }
            Pole[] polaStatku = new Pole[dlugosc];
            for (int i = 0; i < dlugosc; i++)
            {
                int r = wiersz + i * krokWiersza;
                int c = kolumna + i * krokKolumny;
                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        int nr = r + dr;
                        int nc = c + dc;
                        if (nr >= 0 && nr < 10 && nc >= 0 && nc < 10 && plansze[gracz, nr, nc].MaStatek)
                            return;
                    }
                }
                polaStatku[i] = plansze[gracz, r, c];
            }
            Statek statek = new Statek(dlugosc);
            for (int i = 0; i < dlugosc; i++)
            {
                polaStatku[i].Statek = statek;
            }
            ustawioneStatki[gracz, dlugosc]++;
            ustawioneMaszty[gracz] += dlugosc;
            WywolajZmiane();
        }

        public void UsunStatek(int gracz, int wiersz, int kolumna)
        {
            if (graRozpoczeta || potwierdzone[gracz])
                return;
            Statek statek = plansze[gracz, wiersz, kolumna].Statek;
            if (statek == null)
                return;
            for (int r = 0; r < 10; r++)
            {
                for (int c = 0; c < 10; c++)
                {
                    if (plansze[gracz, r, c].Statek == statek)
                        plansze[gracz, r, c].Statek = null;
                }
            }
            ustawioneStatki[gracz, statek.Dlugosc]--;
            ustawioneMaszty[gracz] -= statek.Dlugosc;
            WywolajZmiane();
        }

        public void Potwierdz(int gracz)
        {
            if (graRozpoczeta || potwierdzone[gracz] || !CzyWszystkieStatkiUstawione(gracz))
                return;
            potwierdzone[gracz] = true;
            if (potwierdzone[0] && potwierdzone[1])
            {
                graRozpoczeta = true;
                aktualnaTura = 0;
            }
            WywolajZmiane();
        }

        public void Strzel(int gracz, int wiersz, int kolumna)
        {
            if (!graRozpoczeta || graZakonczona || gracz != aktualnaTura)
                return;
            Pole cel = plansze[1 - gracz, wiersz, kolumna];
            if (cel.CzyStrzelone)
                return;
            cel.CzyStrzelone = true;
            if (cel.MaStatek)
            {
                trafienia[gracz]++;
                if (trafienia[gracz] == liczbaMasztow)
                {
                    graZakonczona = true;
                    zwyciezca = gracz;
                }
            }
            else
            {
                aktualnaTura = 1 - aktualnaTura;
            }
            WywolajZmiane();
        }

        public bool CzyUstawianie { get { return !graRozpoczeta; } }
        public bool CzyGraZakonczona { get { return graZakonczona; } }
        public int AktualnaTura { get { return aktualnaTura; } }

        public bool CzyPotwierdzony(int gracz)
        {
            return potwierdzone[gracz];
        }

        public string PobierzStatus(int gracz)
        {
            if (graZakonczona)
            {
                if (zwyciezca == gracz)
                    return "Wygrales! Zatopiles wszystkie statki!";
                return "Przegrales! Twoje statki zostaly zatopione!";
            }
            if (!graRozpoczeta)
            {
                if (potwierdzone[gracz])
                    return "Gotowe! Czekaj na drugiego gracza.";
                if (CzyWszystkieStatkiUstawione(gracz))
                    return "Wszystkie statki ustawione. Kliknij Gotowe.";
                return "Wybierz statek i kliknij na plansze.";
            }
            if (aktualnaTura == gracz)
                return "Twoja kolej! Strzelaj do przeciwnika.";
            return "Ruch przeciwnika. Poczekaj...";
        }

        private void WywolajZmiane()
        {
            if (Zmieniono != null)
                Zmieniono();
        }
    }

    public class Statek
    {
        public int Dlugosc { get; }

        public Statek(int dlugosc)
        {
            Dlugosc = dlugosc;
        }
    }

    public class Pole
    {
        public Statek Statek { get; set; }
        public bool CzyStrzelone { get; set; }
        public bool MaStatek { get { return Statek != null; } }
    }

    public class GraczViewModel : BazowyViewModel
    {
        private readonly LogikaGry gra;
        private readonly int numerGracza;
        private int wybranaDlugosc = 1;
        private bool poziomo = true;
        private bool trybGumki = false;
        public string TytulOkna { get; }
        public PoleViewModel[] Pola { get; }
        public PrzyciskStatkuViewModel[] PrzyciskiStatkow { get; }
        public string Status { get; private set; }
        public string TekstOrientacji { get; private set; }
        public Visibility WidocznoscUstawiania { get; private set; }
        public bool CzyMoznaPotwierdzic { get; private set; }
        public Brush TloGumki { get; private set; }

        public GraczViewModel(LogikaGry gra, int numerGracza)
        {
            this.gra = gra;
            this.numerGracza = numerGracza;
            TytulOkna = "Gracz " + (numerGracza + 1) + " - Statki";
            Pola = new PoleViewModel[100];
            for (int i = 0; i < 100; i++)
            {
                Pola[i] = new PoleViewModel(i / 10, i % 10);
            }
            PrzyciskiStatkow = new PrzyciskStatkuViewModel[4];
            for (int i = 0; i < 4; i++)
            {
                PrzyciskiStatkow[i] = new PrzyciskStatkuViewModel(i + 1);
            }
            gra.Zmieniono += Odswiez;
            Odswiez();
        }

        public void Odlacz()
        {
            gra.Zmieniono -= Odswiez;
        }

        public void WybierzStatek(int dlugosc)
        {
            if (gra.PobierzPozostaleStatki(numerGracza, dlugosc) <= 0)
                return;
            wybranaDlugosc = dlugosc;
            trybGumki = false;
            OdswiezPanel();
        }

        public void Obroc()
        {
            poziomo = !poziomo;
            OdswiezPanel();
        }

        public void PrzelaczGumke()
        {
            trybGumki = !trybGumki;
            OdswiezPanel();
        }

        public void Potwierdz()
        {
            gra.Potwierdz(numerGracza);
        }

        public void KliknijPole(int wiersz, int kolumna)
        {
            if (gra.CzyUstawianie)
            {
                if (gra.CzyPotwierdzony(numerGracza))
                    return;
                if (trybGumki)
                    gra.UsunStatek(numerGracza, wiersz, kolumna);
                else
                    gra.UstawStatek(numerGracza, wiersz, kolumna, wybranaDlugosc, poziomo);
            }
            else
            {
                gra.Strzel(numerGracza, wiersz, kolumna);
            }
        }

        private void OdswiezPanel()
        {
            bool mozeUstawiac = gra.CzyUstawianie && !gra.CzyPotwierdzony(numerGracza);
            if (poziomo)
                TekstOrientacji = "Poziomo";
            else
                TekstOrientacji = "Pionowo";
            if (mozeUstawiac)
                WidocznoscUstawiania = Visibility.Visible;
            else
                WidocznoscUstawiania = Visibility.Collapsed;
            CzyMoznaPotwierdzic = mozeUstawiac && gra.CzyWszystkieStatkiUstawione(numerGracza);
            if (trybGumki)
                TloGumki = Brushes.LightBlue;
            else
                TloGumki = Brushes.LightGray;
            for (int i = 0; i < 4; i++)
            {
                int dlugosc = i + 1;
                bool aktywne = gra.PobierzPozostaleStatki(numerGracza, dlugosc) > 0;
                bool zaznaczone = wybranaDlugosc == dlugosc && !trybGumki;
                PrzyciskiStatkow[i].Aktualizuj(aktywne, zaznaczone);
            }
            ZglosZmiane("");
        }

        public void Odswiez()
        {
            if (gra.CzyUstawianie && gra.PobierzPozostaleStatki(numerGracza, wybranaDlugosc) <= 0)
            {
                wybranaDlugosc = 0;
                for (int dlugosc = 1; dlugosc <= 4; dlugosc++)
                {
                    if (gra.PobierzPozostaleStatki(numerGracza, dlugosc) > 0)
                    {
                        wybranaDlugosc = dlugosc;
                        break;
                    }
                }
            }
            bool pokazStatki = gra.CzyUstawianie && !gra.CzyPotwierdzony(numerGracza);
            for (int wiersz = 0; wiersz < 10; wiersz++)
            {
                for (int kolumna = 0; kolumna < 10; kolumna++)
                {
                    Pole pole = gra.PobierzWidocznePole(numerGracza, wiersz, kolumna);
                    bool aktywne;
                    if (gra.CzyUstawianie)
                        aktywne = !gra.CzyPotwierdzony(numerGracza);
                    else
                        aktywne = !gra.CzyGraZakonczona && gra.AktualnaTura == numerGracza && !pole.CzyStrzelone;
                    Pola[wiersz * 10 + kolumna].Aktualizuj(pole, pokazStatki, aktywne);
                }
            }
            Status = gra.PobierzStatus(numerGracza);
            OdswiezPanel();
        }
    }

    public class PrzyciskStatkuViewModel : BazowyViewModel
    {
        public int Dlugosc { get; }
        public bool CzyAktywne { get; private set; }
        public Brush Tlo { get; private set; }

        public PrzyciskStatkuViewModel(int dlugosc)
        {
            Dlugosc = dlugosc;
        }

        public void Aktualizuj(bool aktywne, bool zaznaczone)
        {
            Brush noweTlo;
            if (zaznaczone)
                noweTlo = Brushes.LightBlue;
            else
                noweTlo = Brushes.LightGray;
            if (CzyAktywne == aktywne && Tlo == noweTlo)
                return;
            CzyAktywne = aktywne;
            Tlo = noweTlo;
            ZglosZmiane("");
        }
    }

    public class PoleViewModel : BazowyViewModel
    {
        public int Wiersz { get; }
        public int Kolumna { get; }
        public string Tekst { get; private set; }
        public bool CzyAktywne { get; private set; }
        public Brush Tlo { get; private set; }
        public Brush KolorTekstu { get; private set; }

        public PoleViewModel(int wiersz, int kolumna)
        {
            Wiersz = wiersz;
            Kolumna = kolumna;
        }

        public void Aktualizuj(Pole pole, bool pokazStatki, bool aktywne)
        {
            string nowyTekst;
            Brush noweTlo;
            Brush nowyKolor;
            if (pole.CzyStrzelone)
            {
                if (pole.MaStatek)
                {
                    nowyTekst = "X";
                    noweTlo = Brushes.LightCoral;
                    nowyKolor = Brushes.DarkRed;
                }
                else
                {
                    nowyTekst = "·";
                    noweTlo = Brushes.LightGray;
                    nowyKolor = Brushes.Gray;
                }
            }
            else if (pokazStatki && pole.MaStatek)
            {
                nowyTekst = "■";
                noweTlo = Brushes.LightBlue;
                nowyKolor = Brushes.DarkBlue;
            }
            else
            {
                nowyTekst = "";
                noweTlo = Brushes.White;
                nowyKolor = Brushes.Black;
            }
            if (CzyAktywne == aktywne && Tekst == nowyTekst && Tlo == noweTlo && KolorTekstu == nowyKolor)
                return;
            CzyAktywne = aktywne;
            Tekst = nowyTekst;
            Tlo = noweTlo;
            KolorTekstu = nowyKolor;
            ZglosZmiane("");
        }
    }

    public class BazowyViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void ZglosZmiane(string nazwaWlasciwosci)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(nazwaWlasciwosci));
        }
    }
}
