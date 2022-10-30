using SocialNetworkMongoDB;
using SocialNetworkMongoDB.Entities;
using SocialNetworkNeo4J;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialNetworkConsole
{
    public class UserMenu
    {
		private CommandMongoDB commandMongoDB = new CommandMongoDB();
		private CommandNeo4J commandNeo4J = new CommandNeo4J();

		public void runMenu()
        {
            char userInput;
			Console.Write("Sing in? (y/n): ");
			userInput = Console.ReadKey().KeyChar;
			if (userInput != 'y' && userInput == 'n') printDefaultSwitchStringMenu();
			else
			{
				startAuthentication(userInput);
				showMenuOption();
			}
		}

		public void startAuthentication(char userInput)
        {
			bool authenticationRes;
			while (userInput == 'y')
			{
				Console.Clear();
				authenticationRes = checkSuccessOfAuthentication();
				if (authenticationRes)
				{
					break;
				}
				else
				{
					Console.Write("\nWrong username or password! Try again? (y/n): ");
					userInput = Console.ReadKey().KeyChar;
				}
			} 
		}

		private bool checkSuccessOfAuthentication()
		{
			Console.Write("Enter username: ");
			string username = Console.ReadLine();

			Console.Write("Enter password: ");
			string password = readPassword();

			var successAuthentication = commandMongoDB.Authtentificate(username, password);
			commandNeo4J.Authtentificate(username, password);

			return successAuthentication;
		}

		public static string readPassword()
		{
			string password = "";
			ConsoleKeyInfo info = Console.ReadKey(true);
			while (info.Key != ConsoleKey.Enter)
			{
				if (!char.IsControl(info.KeyChar))
				{
					Console.Write("*");
					password += info.KeyChar;
				}
				else if (info.Key == ConsoleKey.Backspace)
				{
					if (!string.IsNullOrEmpty(password))
					{
						password = password.Substring(0, password.Length - 1);
						int pos = Console.CursorLeft;
						Console.SetCursorPosition(pos - 1, Console.CursorTop);
						Console.Write(" ");
						Console.SetCursorPosition(pos - 1, Console.CursorTop);
					}
				}
				info = Console.ReadKey(true);
			}
			Console.WriteLine();
			return password;
		}

		private void showMenuOption()
		{
			char userInput=' ';
			while (userInput != '0') 
			{
				Console.Clear();
				Console.WriteLine("Welcome to the Social Network\n");
				Console.WriteLine("Please select an option:\n");
				Console.WriteLine("1) Posts stream");
				Console.WriteLine("2) My follows");
				Console.WriteLine("3) Search");
				Console.WriteLine("4) Create new user"); 
				Console.WriteLine("5) Delete user"); 
				Console.WriteLine("0) Exit");
				Console.Write("Your choice : ");

				userInput = Console.ReadKey().KeyChar;
				selectShowMenuOption(userInput);
			}
		}

		private void selectShowMenuOption(char userInput)
		{
			switch (userInput)
			{
				case '1':
					showCurrentUserStreamMenu();
					break;
				case '2':
					showFollowsMenu();
					break;
				case '3':
					showSearchMenu();
					break;
				case '4':
					createNewUserMenu();
					break;
				case '5':
					deleteUserMenu();
					break;
				case '0':
					break;
				default:
					printDefaultSwitchStringMenu();
					break;
			}
		}

        private void deleteUserMenu()
        {
			Console.Clear();
			string userName;
			Console.Write("Enter the username to delete : ");
			userName = Console.ReadLine();
			commandMongoDB.DeleteUser(userName);
			//to do
			commandNeo4J.DeleteUser(userName);
		}

        private void createNewUserMenu()
        {
			Console.Clear();
			string userName;
			Console.Write("Your username : ");
			userName = Console.ReadLine();
			string firstName;
			Console.Write("Your first name : ");
			firstName = Console.ReadLine();
			string lastName;
			Console.Write("Your last name : ");
			lastName = Console.ReadLine();
			string email;
			Console.Write("Your email : ");
			email = Console.ReadLine();
			string password;
			Console.Write("Your password : ");
			password = readPassword();
			List<string> follows= new List<string>();

			commandMongoDB.CreateUser(userName, firstName, lastName, password, email, follows);
			commandNeo4J.CreateUser(userName, firstName, lastName, password, email);
		}

        private char readPostOptionsMenuInput(Post post)
        {
			Console.Clear();
			Console.WriteLine(post);
			Console.WriteLine("1) Like\n2) Comments\n3) Next post\n0) Exit");
			Console.Write("Your choice : ");
			return Console.ReadKey().KeyChar;
		}

		private void showCurrentUserStreamMenu()
        {
			Console.Clear();
			var posts = commandMongoDB.GetStreamPosts();
			showStreamMenu(posts);
		}

		private void showStreamMenu(List<Post> posts)
        {
			int index = 0;
			char userInput;
			if (posts.Count == 0)
			{
				Console.WriteLine("Any post yet.");
				return;
			}
			while (index < posts.Count)
			{
				userInput = readPostOptionsMenuInput(posts[index]);
				selectStreamMenuInput(userInput, posts[index], ref index);
				if (userInput == '0') break;
			}
			if (index == posts.Count)
			{
				Console.WriteLine("\nThat was last post.");
				Console.ReadLine();
			}
		}


		private void showSearchMenu()
		{
			char userInput=' ';
			while (userInput != '0')
			{
				Console.Clear();
				Console.WriteLine("1) Search\n0) Exit");
				Console.Write("Your choice : ");
				userInput = Console.ReadKey().KeyChar;
				selectSearchMenuInput(userInput);

			} 
		}

		private void selectSearchMenuInput(char userInput)
		{
			switch (userInput)
			{
				case '1':
					searchUserCommandMenu();
					break;
				case '0':
					break;
				default:
					printDefaultSwitchStringMenu();
					break;
			}
		}

		private void searchUserCommandMenu()
		{
			string username;
			Console.Write("\nEnter username :");
			username = Console.ReadLine();
			var foundUser = commandMongoDB.FindUser(username);
			if (foundUser != null)
			{
				showSearchedUserInfoMenu(foundUser);
			}
		}

		private void showSearchedUserInfoMenu(User user)
		{
			char userInput=' ';
			while (userInput != '0') 
			{
				Console.Clear();
				Console.WriteLine("Profile:");
				Console.WriteLine(user);
				ExistRelationshipMenu(user.UserName);
				printUserIsFollowedMenu(user);
				userInput = readSearchedUserOptionMenuInput();
				selectSearchedUserMenuInput(userInput, user);
			}
		}

		private char readSearchedUserOptionMenuInput()
        {
			Console.WriteLine("\n1) Posts\n2) Follow or UnFollow\n0) Exit");
			Console.Write("Your choice : ");
			return Console.ReadKey().KeyChar;
		}

		private void ExistRelationshipMenu(string username)
        {
			var existRelationship = commandNeo4J.SearchRelationshipOfUser(username);

			if (existRelationship.Count() != 0) 
			{
				Console.WriteLine("\nYou have relationship with this user )");
				Console.WriteLine($"The distance to this user : {commandNeo4J.ShortestPathToSearthedUser(username)}");
			}
			else
			{
				Console.WriteLine("\nYou haven`t relationship with this user (");
				Console.WriteLine($"The distance to this user : {commandNeo4J.ShortestPathToSearthedUser(username)}");
			}
		}

		private void printUserIsFollowedMenu(User user)
        {
			if (commandMongoDB.CheckUserIsFollowed(user.UserName )==true)
			{
				Console.WriteLine("You follow this user");
			}
			else
			{
				Console.WriteLine("You not follow this user");
			}
		}

		private void userFolloweOptionMenu(User user)
		{
			
			if (commandMongoDB.CheckUserIsFollowed(user.UserName)==true)
			{
				commandMongoDB.UnFollow(user.UserName);
				commandNeo4J.DeleteRelationshipUserFollower(user.UserName);
				Console.WriteLine($"\nYou not follow user : {user.UserName}");
			}
			else
			{
				commandMongoDB.Follow(user.UserName);
				commandNeo4J.CreateRelationshipUserFollower(user.UserName);
				Console.WriteLine($"\nYou follow user  : {user.UserName}");
			}

		}

		private void selectSearchedUserMenuInput(char userInput, User user)
		{
			switch (userInput)
			{
				case '1':
					showSearchedUserStream(user);
					break;
				case '2':
					userFolloweOptionMenu(user);
					break;
				case '0':
					break;
				default:
					printDefaultSwitchStringMenu();
					break;
			}
		}

		private void showSearchedUserStream(User user)
		{
			Console.Clear();
			var posts = commandMongoDB.GetStreamPosts(user.UserName);
			showStreamMenu(posts);
		}

		private char readFollowOptionInput()
		{
			Console.WriteLine("\n\n1) UnFollow\n2) Posts\n0) Exit");
			Console.Write("Your choice : ");
			return Console.ReadKey().KeyChar;
		}

		private void showFollowsMenu()
		{
			char userInput =' ';
			List<User> follows;
			while (userInput != '0')
			{
				Console.Clear();
				follows = commandMongoDB.GetFollows();
				Console.WriteLine("Follows:\n");
				if (follows.Count() == 0)
				{
					Console.WriteLine("You do not have the followers yet");
				}
				else
				{
					foreach (var f in follows)
					{
						Console.WriteLine(f);
					}
				}
				userInput = readFollowOptionInput();
				selecFollowsMenuInput(userInput);

			} 
		}

		private void selecFollowsMenuInput(char userInput)
		{
			switch (userInput)
			{
				case '1':
					unFollowUserMenuCommand();
					break;
				case '2':
					showFollowStreamMenu();
					break;
				case '0':
					break;
				default:
					printDefaultSwitchStringMenu();
					break;
			}
		}

		private void unFollowUserMenuCommand()
		{
			string selected;
			bool? success;
			Console.Write("\nWrite username : ");
			selected = Console.ReadLine();
			success = commandMongoDB.UnFollow(selected);
			commandNeo4J.DeleteRelationshipUserFollower(selected);
			if (success==true)
			{
				Console.WriteLine($"Unfollowed user: {selected}");
			}
		}


		private void showFollowStreamMenu()
		{
			string selected;

			Console.Write("\nWrite username : ");
			selected = Console.ReadLine();

			if (commandMongoDB.CheckUserIsFollowed(selected)==true)
			{
				var posts = commandMongoDB.GetStreamPosts(selected);
				showStreamMenu(posts);
			}
			else
			{
				Console.WriteLine("Wrong usernam!");
			}
		}

		private void selectStreamMenuInput(char userInput, Post post, ref int index)
		{
			switch (userInput)
			{
				case '1':
					commandMongoDB.LikePost(post);
					break;
				case '2':
					showCommentsMenu(post);
					break;
				case '3':
					index++;
					break;
				case '0':
					break;
				default:
					printDefaultSwitchStringMenu();
					break;
			}
		}

		private char readCommentOptionsMenuInput()
		{
			Console.WriteLine("1) Write comment\n0) Exit");
			Console.Write("Your choice : ");
			return Console.ReadKey().KeyChar;
		}

		private void showCommentsMenu(Post post)
		{
			char userInput=' ';
			while (userInput != '0')
            {
				Console.Clear();
				foreach (Comment comment in post.Comments.OrderBy(c => c.CreationDate))
				{
					Console.WriteLine(comment);
					Console.WriteLine();
				}
				userInput = readCommentOptionsMenuInput();
				selectCommentsMenuInput(post, userInput);
			}

		}

		private void selectCommentsMenuInput(Post post, char userInput)
		{
			switch (userInput)
			{
				case '1':
					WriteCommentMenu(post);
					break;
				case '0':
					break;
				default:
					printDefaultSwitchStringMenu();
					break;
			}
		}

		private void printDefaultSwitchStringMenu()
        {
			Console.WriteLine("\nWrong command selected");
		}

		private void WriteCommentMenu(Post post)
		{
			string userComment;
			Console.Write("Your comment : ");
			userComment = Console.ReadLine();
			commandMongoDB.WriteComment(post, userComment);
		}
	}
}


