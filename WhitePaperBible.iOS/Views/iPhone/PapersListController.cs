﻿
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using WhitePaperBible.Core.Views;
using MonkeyArms;
using System.Collections.Generic;
using WhitePaperBible.Core.Models;
using WhitePaperBible.iOS.Invokers;
using IOS.Util;

namespace WhitePaperBible.iOS
{
	public partial class PapersListController : UIViewController, IPapersListView
	{
		PapersView papersList;

		LoginRequiredController LoginRequiredView;

		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public PapersListController ()
			: base (UserInterfaceIdiomIsPhone ? "PapersListController_iPhone" : "PapersListController_iPad", null)
		{
			this.Title = "Papers";

//			this.Filter = new Invoker ();
//			this.OnPaperSelected = new Invoker ();
			this.AddPaper = new Invoker ();
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
			AddDialog ();
			AnalyticsUtil.TrackScreen (this.Title);

			NavigationItem.SetRightBarButtonItem (
				new UIBarButtonItem ("Add Paper", UIBarButtonItemStyle.Plain, (sender, args)=> {
					AddPaper.Invoke();
				})
				, true
			);

			UpdateTopConstraint ();

//			if (![self respondsToSelector:@selector(topLayoutGuide)]) {
//				self.topConstraint.constant = self.topConstraint.constant - 64;
//			} 
		}

		void UpdateTopConstraint ()
		{
			if(this.ListTopConstraint != null){
				this.ListTopConstraint.Constant = UIApplication.SharedApplication.StatusBarFrame.Height + this.NavigationController.NavigationBar.Frame.Height;

				if(LoginRequiredView != null){
					LoginRequiredView.TopConstraint.Constant = this.ListTopConstraint.Constant;
				}
			}
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			DI.RequestMediator (this);
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			DI.DestroyMediator (this);

		}

		void AddDialog ()
		{
			papersList = new PapersView (this.NavigationController);
//			papersList.Filter.Invoked += OnFilterInvoked;
//			papersList.OnPaperSelected.Invoked += HandlePaperSelected;
			ListContainer.AddSubview (papersList.TableView);
		}

//		void OnFilterInvoked (object sender, EventArgs e)
//		{
//			throw new NotImplementedException ();
//		}

//		void HandlePaperSelected (object sender, EventArgs e)
//		{
//			throw new NotImplementedException ();
//		}

		public void SetPapers (List<Paper> papers)
		{
			papersList.SetPapers (papers);
		}

//		public Invoker Filter {
//			get;
//			private set;
//		}
//
//		public Invoker OnPaperSelected {
//			get;
//			private set;
//		}

		public Invoker AddPaper {
			get;
			private set;
		}

		public void AddPaperEditView()
		{
			var addPaperView = new EditPaperView();
			var editNav = new UINavigationController (addPaperView);
			this.PresentViewController (editNav, true, null);
		}

		public string SearchPlaceHolderText {
			get{
				return papersList.SearchPlaceHolderText;
			}
			set {
				papersList.SearchPlaceHolderText = value;
			}
		}

		public string SearchQuery {
			get{
				return papersList.SearchQuery;
			}
			set {
				papersList.SearchQuery = value;
			}
		}

		public Paper SelectedPaper {
			get{
				return papersList.SelectedPaper;
			}
			set {
				papersList.SelectedPaper = value;
			}
		}

		public void PromptForLogin ()
		{
			if (LoginRequiredView == null) {
				CreateLoginRequiredView ();
				LoginRequiredView.View.Hidden = false;
			}
		}

		public void ShowLoginForm ()
		{
			var loginView = new LoginViewController ();
			loginView.LoginFinished.Invoked += (object sender, EventArgs e) => {
				(e as LoginFinishedInvokerArgs).Controller.DismissViewController (true, null);
			};

			this.PresentViewController (loginView, true, null);
		}

		protected void CreateLoginRequiredView ()
		{
			LoginRequiredView = new LoginRequiredController ();
			View.AddSubview (LoginRequiredView.View);
			View.BringSubviewToFront (LoginRequiredView.View);
			LoginRequiredView.LoginRegister.Invoked += (object sender, EventArgs e) => ShowLoginForm ();
			LoginRequiredView.CancelRegister.Invoked += (object sender, EventArgs e) => DismissLoginPrompt();
			LoginRequiredView.View.Hidden = true;
		}

		public void DismissLoginPrompt()
		{
			if (LoginRequiredView != null && !LoginRequiredView.View.Hidden) {
				LoginRequiredView.View.Hidden = true;
				LoginRequiredView = null;
			}
		}

		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			UpdateTopConstraint ();
			base.DidRotate (fromInterfaceOrientation);
		}
	}
}
