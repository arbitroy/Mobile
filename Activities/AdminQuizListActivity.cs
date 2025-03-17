using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using Mobile.Models;
using Mobile.Services;
using Android.Content;
using AndroidX.RecyclerView.Widget;
using Android.Views;
using System.Linq;
using System.Threading.Tasks;

namespace Mobile.Activities
{
    [Activity(Label = "Manage Quizzes")]
    public class AdminQuizListActivity : BaseAuthenticatedActivity
    {
        private RecyclerView _quizRecyclerView;
        private ProgressBar _loadingProgressBar;
        private TextView _emptyTextView;
        private Button _createQuizButton;
        private SearchView _searchView;
        private List<Quiz> _quizzes;
        private AdminQuizAdapter _adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the layout resource
            SetContentView(Resource.Layout.activity_admin_quiz_list);

            // Initialize UI elements
            _quizRecyclerView = FindViewById<RecyclerView>(Resource.Id.quizRecyclerView);
            _loadingProgressBar = FindViewById<ProgressBar>(Resource.Id.loadingProgressBar);
            _emptyTextView = FindViewById<TextView>(Resource.Id.emptyTextView);
            _createQuizButton = FindViewById<Button>(Resource.Id.createQuizButton);
            _searchView = FindViewById<SearchView>(Resource.Id.searchView);

            // Set up RecyclerView
            _quizRecyclerView.SetLayoutManager(new LinearLayoutManager(this));

            // Set up event handlers
            _createQuizButton.Click += OnCreateQuizButtonClick;
            _searchView.QueryTextChange += OnSearchQueryTextChange;

            // Load quizzes
            LoadQuizzesAsync();
        }

        protected override void OnRolesLoaded()
        {
            // Ensure user has admin access
            if (!IsAdmin)
            {
                Toast.MakeText(this, "You need administrator privileges to access this page", ToastLength.Long).Show();
                Finish();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            // Reload quizzes when returning to this activity
            LoadQuizzesAsync();
        }

        private async void LoadQuizzesAsync()
        {
            try
            {
                // Show loading indicator
                _loadingProgressBar.Visibility = ViewStates.Visible;
                _emptyTextView.Visibility = ViewStates.Gone;
                _quizRecyclerView.Visibility = ViewStates.Gone;

                // Get quizzes from API
                _quizzes = await ApiService.GetQuizzesAsync();

                if (_quizzes.Count == 0)
                {
                    // Show empty message
                    _emptyTextView.Visibility = ViewStates.Visible;
                    _quizRecyclerView.Visibility = ViewStates.Gone;
                }
                else
                {
                    // Setup and show RecyclerView
                    _adapter = new AdminQuizAdapter(this, _quizzes);
                    _quizRecyclerView.SetAdapter(_adapter);

                    _emptyTextView.Visibility = ViewStates.Gone;
                    _quizRecyclerView.Visibility = ViewStates.Visible;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle authentication errors
                HandleAuthError();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to load quizzes: {ex.Message}", ToastLength.Long).Show();
                _emptyTextView.Text = "Error loading quizzes. Please try again.";
                _emptyTextView.Visibility = ViewStates.Visible;
                _quizRecyclerView.Visibility = ViewStates.Gone;
            }
            finally
            {
                // Hide loading indicator
                _loadingProgressBar.Visibility = ViewStates.Gone;
            }
        }

        private void OnCreateQuizButtonClick(object sender, EventArgs e)
        {
            // Navigate to create quiz activity
            var intent = new Intent(this, typeof(AdminCreateQuizActivity));
            StartActivity(intent);
        }

        private void OnSearchQueryTextChange(object sender, SearchView.QueryTextChangeEventArgs e)
        {
            // Filter quizzes by title
            if (_adapter != null)
            {
                _adapter.Filter(e.NewText);
            }
        }

        private async Task<bool> DeleteQuizAsync(int quizId)
        {
            try
            {
                // Show loading dialog
                var progressDialog = new ProgressDialog(this);
                progressDialog.SetMessage("Deleting quiz...");
                progressDialog.SetCancelable(false);
                progressDialog.Show();

                try
                {
                    // Call API to delete quiz
                    await ApiService.DeleteQuizAsync(quizId);
                    return true;
                }
                finally
                {
                    // Hide loading dialog
                    if (progressDialog.IsShowing)
                    {
                        progressDialog.Dismiss();
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                HandleAuthError();
                return false;
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, $"Failed to delete quiz: {ex.Message}", ToastLength.Long).Show();
                return false;
            }
        }

        private void HandleAuthError()
        {
            TokenManager.ClearToken(this);
            Toast.MakeText(this, "Your session has expired. Please log in again.", ToastLength.Long).Show();
            RedirectToLogin();
        }

        // RecyclerView adapter for quizzes
        private class AdminQuizAdapter : RecyclerView.Adapter
        {
            private readonly AdminQuizListActivity _activity;
            private readonly List<Quiz> _allQuizzes;
            private List<Quiz> _filteredQuizzes;

            public AdminQuizAdapter(AdminQuizListActivity activity, List<Quiz> quizzes)
            {
                _activity = activity;
                _allQuizzes = quizzes;
                _filteredQuizzes = new List<Quiz>(quizzes);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.layout_admin_quiz_list_item, parent, false);
                return new QuizViewHolder(itemView);
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                QuizViewHolder viewHolder = holder as QuizViewHolder;
                Quiz quiz = _filteredQuizzes[position];

                // Set quiz details
                viewHolder.TitleTextView.Text = quiz.Title;
                viewHolder.DescriptionTextView.Text = quiz.Description;
                viewHolder.QuestionCountTextView.Text = $"{quiz.QuestionCount} questions";
                viewHolder.TimeLimitTextView.Text = $"{quiz.TimeLimit} min";

                // Set up event handlers for buttons
                viewHolder.EditButton.Click += (sender, e) => {
                    // Navigate to edit quiz activity
                    var intent = new Intent(_activity, typeof(AdminEditQuizActivity));
                    intent.PutExtra("QuizId", quiz.Id);
                    _activity.StartActivity(intent);
                };

                viewHolder.DeleteButton.Click += async (sender, e) => {
                    // Show confirmation dialog
                    var alertDialog = new AlertDialog.Builder(_activity);
                    alertDialog.SetTitle("Delete Quiz");
                    alertDialog.SetMessage($"Are you sure you want to delete '{quiz.Title}'? This action cannot be undone.");
                    alertDialog.SetPositiveButton("Delete", async (senderAlert, args) => {
                        // Delete quiz
                        bool success = await _activity.DeleteQuizAsync(quiz.Id);
                        if (success)
                        {
                            // Remove from lists and update UI
                            int adapterPosition = viewHolder.AdapterPosition;
                            if (adapterPosition != RecyclerView.NoPosition)
                            {
                                Quiz quizToRemove = _filteredQuizzes[adapterPosition];
                                _filteredQuizzes.RemoveAt(adapterPosition);
                                _allQuizzes.Remove(quizToRemove);
                                NotifyItemRemoved(adapterPosition);

                                // Show empty message if no more quizzes
                                if (_filteredQuizzes.Count == 0)
                                {
                                    _activity._emptyTextView.Visibility = ViewStates.Visible;
                                    _activity._quizRecyclerView.Visibility = ViewStates.Gone;
                                }

                                Toast.MakeText(_activity, "Quiz deleted successfully", ToastLength.Short).Show();
                            }
                        }
                    });
                    alertDialog.SetNegativeButton("Cancel", (senderAlert, args) => {
                        // Do nothing
                    });
                    alertDialog.Show();
                };
            }

            public override int ItemCount => _filteredQuizzes.Count;

            public void Filter(string query)
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    // If query is empty, show all quizzes
                    _filteredQuizzes = new List<Quiz>(_allQuizzes);
                }
                else
                {
                    // Filter quizzes by title or description containing the query (case-insensitive)
                    string lowerQuery = query.ToLower();
                    _filteredQuizzes = _allQuizzes
                        .Where(q => q.Title.ToLower().Contains(lowerQuery) ||
                                   (q.Description != null && q.Description.ToLower().Contains(lowerQuery)))
                        .ToList();
                }

                // Update UI based on filter results
                if (_filteredQuizzes.Count == 0 && _allQuizzes.Count > 0)
                {
                    _activity._emptyTextView.Text = "No quizzes match your search.";
                    _activity._emptyTextView.Visibility = ViewStates.Visible;
                }
                else
                {
                    _activity._emptyTextView.Visibility = ViewStates.Gone;
                }

                NotifyDataSetChanged();
            }

            // ViewHolder for quiz items
            private class QuizViewHolder : RecyclerView.ViewHolder
            {
                public TextView TitleTextView { get; }
                public TextView DescriptionTextView { get; }
                public TextView QuestionCountTextView { get; }
                public TextView TimeLimitTextView { get; }
                public Button EditButton { get; }
                public Button DeleteButton { get; }

                public QuizViewHolder(View itemView) : base(itemView)
                {
                    TitleTextView = itemView.FindViewById<TextView>(Resource.Id.titleTextView);
                    DescriptionTextView = itemView.FindViewById<TextView>(Resource.Id.descriptionTextView);
                    QuestionCountTextView = itemView.FindViewById<TextView>(Resource.Id.questionCountTextView);
                    TimeLimitTextView = itemView.FindViewById<TextView>(Resource.Id.timeLimitTextView);
                    EditButton = itemView.FindViewById<Button>(Resource.Id.editButton);
                    DeleteButton = itemView.FindViewById<Button>(Resource.Id.deleteButton);
                }
            }
        }
    }
}