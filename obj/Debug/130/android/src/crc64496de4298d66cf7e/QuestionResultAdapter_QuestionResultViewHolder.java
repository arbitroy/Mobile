package crc64496de4298d66cf7e;


public class QuestionResultAdapter_QuestionResultViewHolder
	extends androidx.recyclerview.widget.RecyclerView.ViewHolder
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("Mobile.Activities.QuestionResultAdapter+QuestionResultViewHolder, Mobile", QuestionResultAdapter_QuestionResultViewHolder.class, __md_methods);
	}


	public QuestionResultAdapter_QuestionResultViewHolder (android.view.View p0)
	{
		super (p0);
		if (getClass () == QuestionResultAdapter_QuestionResultViewHolder.class) {
			mono.android.TypeManager.Activate ("Mobile.Activities.QuestionResultAdapter+QuestionResultViewHolder, Mobile", "Android.Views.View, Mono.Android", this, new java.lang.Object[] { p0 });
		}
	}

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
