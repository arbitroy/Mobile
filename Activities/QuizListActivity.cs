using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mobile.Models;
using Mobile.Services;

namespace Mobile.Activities
{
    [Activity(Label = "Available Quizzes")]
    public class QuizListActivity : Activity
    {
        private ListView _quizListView;
        private ApiService _apiService;
        private List<Quiz> _quizzes;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the layout resource
            SetContentView(Resource.Layout.activity_quiz_list);

            // Initialize UI elements
            _quizListView = FindViewById<ListView>(Resource.Id.quizListView);

            // Initialize service with context to access shared preferences
            _apiService = new ApiService(this);

            // Load quizzes
            LoadQuizzesAsync();

            // Set up event handlers
            _quizListView.ItemClick += OnQuizItemClick;
        }

        private async void LoadQuizzesAsync()
        {
            try
            {
                // Show loading indicator
                FindViewById<ProgressBar>(Resource.Id.loadingProgressBar).Visibility = Android.Views.ViewStates.Visible;

                // Get quizzes from API
                _quizzes = await _apiService.GetQuizzesAsync();

                if (_quizzes.Count == 0)
                {
                    // Show empty message
                    FindViewById<TextView>(Resource.Id.emptyTextView).Visibility = Android.Views.ViewStates.Visible;
                    return;
                }

                // Create adapter for the list view
                var adapter = new ArrayAdapter<string>(
                    this,
                    Android.Resource.Layout.SimpleListItem1,
                    _quizzes.ConvertAll(q => q.Title)
                );

                // Set adapter to the list view
                _quizListView.Adapter = adapter;
            }
            catch (Exception ex)
            {
                // Check if this is an authentication error
                if (ex.Message.Contains("must be logged in"))
                {
                    // Clear invalid token and redirect to login
                    TokenManager.ClearToken(this);
                    Toast.MakeText(this, "Your session has expired. Please log in again.", ToastLength.Long).Show();

                    var intent = new Android.Content.Intent(this, typeof(MainActivity));
                    StartActivity(intent);
                    Finish();
                    return;
                }

                Toast.MakeText(this, $"Failed to load quizzes: {ex.Message}", ToastLength.Long).Show();
            }
            finally
            {
                // Hide loading indicator
                FindViewById<ProgressBar>(Resource.Id.loadingProgressBar).Visibility = Android.Views.ViewStates.Gone;
            }
        }

        private void OnQuizItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var selectedQuiz = _quizzes[e.Position];

            // Navigate to quiz detail activity
            var intent = new Android.Content.Intent(this, typeof(QuizDetailActivity));
            intent.PutExtra("QuizId", selectedQuiz.Id);
            intent.PutExtra("QuizTitle", selectedQuiz.Title);
            StartActivity(intent);
        }
    }
}