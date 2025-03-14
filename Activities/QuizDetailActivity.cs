using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Threading.Tasks;
using Mobile.Models;
using Mobile.Services;

namespace Mobile.Activities
{
    [Activity(Label = "Quiz Details")]
    public class QuizDetailActivity : Activity
    {
        private TextView _titleTextView;
        private TextView _descriptionTextView;
        private TextView _timeLimitTextView;
        private TextView _questionCountTextView;
        private Button _startQuizButton;
        private ApiService _apiService;
        private int _quizId;
        private QuizDetail _quizDetail;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the layout resource
            SetContentView(Resource.Layout.activity_quiz_detail);

            // Get quiz ID from intent
            _quizId = Intent.GetIntExtra("QuizId", 0);
            var quizTitle = Intent.GetStringExtra("QuizTitle");

            // Set activity title to quiz title
            Title = quizTitle;

            // Initialize UI elements
            _titleTextView = FindViewById<TextView>(Resource.Id.titleTextView);
            _descriptionTextView = FindViewById<TextView>(Resource.Id.descriptionTextView);
            _timeLimitTextView = FindViewById<TextView>(Resource.Id.timeLimitTextView);
            _questionCountTextView = FindViewById<TextView>(Resource.Id.questionCountTextView);
            _startQuizButton = FindViewById<Button>(Resource.Id.startQuizButton);

            // Initialize service with context to access shared preferences
            _apiService = new ApiService(this);

            // Load quiz details
            LoadQuizDetailAsync();

            // Set up event handlers
            _startQuizButton.Click += OnStartQuizButtonClick;
        }

        private async void LoadQuizDetailAsync()
        {
            try
            {
                // Show loading indicator
                FindViewById<ProgressBar>(Resource.Id.loadingProgressBar).Visibility = Android.Views.ViewStates.Visible;

                // Get quiz detail from API
                _quizDetail = await _apiService.GetQuizDetailAsync(_quizId);

                // Update UI with quiz details
                _titleTextView.Text = _quizDetail.Title;
                _descriptionTextView.Text = _quizDetail.Description;
                _timeLimitTextView.Text = $"Time Limit: {_quizDetail.TimeLimit} minutes";
                _questionCountTextView.Text = $"Questions: {_quizDetail.QuestionCount}";
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to load quiz details: {ex.Message}", ToastLength.Long).Show();
                Finish(); // Return to previous activity on error
            }
            finally
            {
                // Hide loading indicator
                FindViewById<ProgressBar>(Resource.Id.loadingProgressBar).Visibility = Android.Views.ViewStates.Gone;
            }
        }

        private void OnStartQuizButtonClick(object sender, EventArgs e)
        {
            // Navigate to quiz-taking activity
            var intent = new Android.Content.Intent(this, typeof(TakeQuizActivity));
            intent.PutExtra("QuizId", _quizId);
            intent.PutExtra("QuizTitle", _quizDetail.Title);
            StartActivity(intent);
        }
    }
}