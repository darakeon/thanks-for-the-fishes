﻿using System;
using System.IO;
using System.Linq;

namespace TF2.MainConsole
{
	internal class Program
	{
		public static void Main(String[] args)
		{
			var sourceDirectory = args[0];

			processCommits(sourceDirectory);

			Console.Read();
		}

		private static void processCommits(String sourceDirectory)
		{
			var git2Hg = new Git2Hg(sourceDirectory);

			var populated = git2Hg.PopulateCommitList(showSequenceError);
			if (!populated) return;

			File.ReadAllLines("disclaimer.txt")
				.ToList().ForEach(Console.WriteLine);

			var shouldGoAhead = askGoAhead(git2Hg.CommitCount);
			if (!shouldGoAhead) return;

			var succeded = git2Hg.CommitOnGit(askOverwriteGit, notifyNewCount, askCommit, warnReversal);
			if (!succeded) return;

			finishSucceded(git2Hg.CommitCount);
		}

		private static void showSequenceError(Int32 expected, Int32 received)
		{
			Console.WriteLine("Error on parsing commits:");
			Console.WriteLine($"Commit with position {received} in repository is in position {expected} in list.");
			Console.WriteLine($"Maybe the position {expected} in repository was not parsed.");
		}

		private static Boolean askGoAhead(Int32 commitCount)
		{
			var answer = ask(() =>
			{
				Console.WriteLine($"Total commits: {commitCount}");
				Console.Write("Do you want to go ahead? (y/n) ");
			}, "y", "n");

			if (answer.ToLower() != "n") return true;

			Console.WriteLine();
			Console.WriteLine("Ok. See ya! o/");
			return false;
		}

		private static Boolean askOverwriteGit()
		{
			var answer = ask(() =>
			{
				Console.WriteLine("Git already exists at this directory.");
				Console.Write("Do you want to overwrite or keep commiting on it? (o/k) ");
			}, "o", "k");

			return answer.ToLower() == "o";
		}

		private static void notifyNewCount(Int32 total, Int32 diff)
		{
			Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"From log, there is {diff} commit(s) already recorded on git.");
			Console.WriteLine($"New commits count: {total - diff}.");
			Console.ResetColor();
		}

		private static Boolean askCommit(String title)
		{
			var answer = ask(() =>
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(title);
				Console.ResetColor();
				Console.Write("Commit on git? (y/n) ");
			}, "y", "n");

			if (answer.ToLower() != "n") return true;

			Console.WriteLine("Process stopped.");
			return false;
		}

		private static void warnReversal()
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("The next commit is not a child of this one.");
			Console.WriteLine("This is due to Hg having not rebase, and some commit reversal was done here.");
			Console.WriteLine("You can rebase and delete reversal OR squash it to Hg merge commit, to keep log cleaner.");
			Console.ResetColor();
		}

		private static void finishSucceded(Int32 commitCount)
		{
			Console.WriteLine();
			Console.WriteLine($"Oh, my god! All {commitCount} commits done!");
			Console.WriteLine("Mercurial, farewell and thanks for the fish!");
		}

		private static string ask(Action question, params String[] acceptedAnswers)
		{
			String answer;

			do
			{
				Console.WriteLine();
				question();
				answer = Console.ReadLine()?.ToLower();
			}
			while (!acceptedAnswers.Contains(answer));

			return answer;
		}
	}
}

