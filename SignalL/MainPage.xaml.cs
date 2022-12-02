using Firebase.Storage;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

namespace SignalL;

public partial class MainPage : ContentPage
{
    bool lstbool = false;

    string str;
    string[] abc;
    int k = 0;
    int j = -1;
    string doc;
    string pat;

    private const string BasePath = "https://signlanguage-51654-default-rtdb.firebaseio.com/";   //FB URL
    private const string FirebaseSecret = "4oFpHzS8P2EUYLZlUSL6ZwjczyJBtoWV6n9GDrAt";    // FB 비번
    private static FirebaseClient _client;


    public MainPage()
    {
        InitializeComponent();

        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = FirebaseSecret,
            BasePath = BasePath
        };
        _client = new FirebaseClient(config);

        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            getfb();
            return true;
        });
    }



    /// <summary>
    /// firebase에서 진료내용(대화)데이터를 가져온다
    /// </summary>
    private async void getfb() 
    {
        FirebaseResponse response = await _client.GetAsync("MAUI");

        str = response.Body.ToString();
        if (str.Length > 7)
        {
            str = str.Remove(0, 7);
            str = str.Replace("\"]", "");
            abc = str.Split("\",\"");
            lstview.ItemsSource = abc;

            for (int i = 0; i < abc.Length; i++)
            {
                if (abc[i].Contains("\",null,\"의사: "))
                {
                    abc[i] = abc[i].Replace("\",null,\"의사: ", " ");
                }
                else if (abc[i].Contains("\",null,\"환자: "))
                {
                    abc[i] = abc[i].Replace("\",null,\"환자: ", " ");
                }

                if (abc[i].Contains("의사: "))
                {
                    doc = abc[i].Replace("의사: ", "");
                }
                else if (abc[i].Contains("환자: "))
                {
                    pat = abc[i].Replace("환자: ", "");
                }
            }
        }
        else
        {
            doc = "";
            pat = "";
        }

        lbldoc.Text = doc;
        lblpat.Text = pat;

    }



    /// <summary>
    /// firebase에 촬영한 영상의 파일이름을 보낸다.
    /// </summary>
    private async void setfb(string name, string keyname)
    {
        FirebaseResponse response = await _client.GetAsync(keyname);
        _client.Set<string>("title", name);
        _client.Set<int>(keyname + "/flag", 1);
    }



    /// <summary>
    /// 환자의 영상을 촬영한다.
    /// </summary>
    public async void RecordVideo()
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            FileResult photo = await MediaPicker.Default.CaptureVideoAsync();

            if (photo != null)
            {
                string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                using Stream sourceStream = await photo.OpenReadAsync();

                var task = new FirebaseStorage("signlanguage-51654.appspot.com",
                new FirebaseStorageOptions
                {
                    ThrowOnCancel = true
                })
                .Child("Patient")
                .Child(ClassId = photo.FileName.ToString())
                .PutAsync(await photo.OpenReadAsync());

                FirebaseResponse response = await _client.GetAsync("title");
                setfb(photo.FileName.ToString(), "video");
            }
        }
    }



    /// <summary> //////////////////////////////////////////////////////////////////////////////////////////////////////카메라 버튼
    /// 환자 녹화 버튼을 누르면 촬영 시작
    /// </summary>
    private void camera_Clicked(object sender, EventArgs e) //환자 녹화
    {
        RecordVideo();
    }



    /// <summary>
    /// 의사의 영상을 촬영한다. (음성을 녹음한다.)
    /// </summary>
    public async void RecordVoice()
    {
        if (MediaPicker.Default.IsCaptureSupported)
        {
            FileResult photo = await MediaPicker.Default.CaptureVideoAsync();

            if (photo != null)
            {
                string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                using Stream sourceStream = await photo.OpenReadAsync();

                var task = new FirebaseStorage("signlanguage-51654.appspot.com",
                new FirebaseStorageOptions
                {
                    ThrowOnCancel = true
                })
                .Child("Doctor")
                .Child(ClassId = photo.FileName.ToString())
                .PutAsync(await photo.OpenReadAsync());


                FirebaseResponse response = await _client.GetAsync("title");
                setfb(photo.FileName.ToString(), "voice");
            }
        }
    }



    /// <summary> //////////////////////////////////////////////////////////////////////////////////////////////////녹음 버튼
    /// 환자 녹음 버튼을 누르면 촬영(녹음) 시작
    /// </summary>
    private void record_Clicked(object sender, EventArgs e)
    {
        RecordVoice();
    }



    /// <summary> ///////////////////////////////////////////////////////////////////////////////////////////////////파일 버튼
    /// 파일 버튼을 누르면 firebase에 저장된 대화 데이터를 가져온다.
    /// </summary>
    private void File_Clicked(object sender, EventArgs e)
    {
        getfb();

        if (lstbool == true)
        {
            lstbool = false;
            lstview.IsVisible = false;
            Frame1.IsVisible = true;
            Frame2.IsVisible = true;
        }
        else
        {
            lstbool = true;
            lstview.IsVisible = true;
            Frame1.IsVisible = false;
            Frame2.IsVisible = false;
        }

    }



    /// <summary> ///////////////////////////////////////////////////////////////////////////////////////////////////휴지통 버튼
    /// 휴지통버튼을 누르면 저장된 모든 데이터를 삭제한다.
    /// </summary>
    private void trash_Clicked(object sender, EventArgs e)
    {
        _client.Set<string>("MAUI", "");
        _client.Set<int>("title", 0);
        _client.Set<int>("num", 1);
        _client.Set<int>("video/flag", 0);
        _client.Set<int>("voice/flag", 0);

        str = "";
        abc = null;
        k = 0;
        j = -1;
        doc = "";
        pat = "";

        lstview.ItemsSource = abc;

        lbldoc.Text = doc;
        lblpat.Text = pat;


        if (lstbool == true)
        {
            lstbool = false;
            lstview.IsVisible = false;
            Frame1.IsVisible = true;
            Frame2.IsVisible = true;
        }

        getfb();

    }
}