using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TF2.MainConsole
{
	internal class Git : Terminal
	{
		private readonly String sourceDirectory;
		public const String DEFAULT_BRANCH = "master";
		private String currentBranch = DEFAULT_BRANCH;
		private readonly IList<String> branches = new List<String> { DEFAULT_BRANCH };
		private String alreadyCommitedFile => Path.Combine(sourceDirectory, ".git", "hg-already-commited.txt");

		public Git(String sourceDirectory) : base(sourceDirectory)
		{
			this.sourceDirectory = sourceDirectory;
		}
		
		internal delegate Boolean AskOverwrite();
		internal delegate void NotifyNewCount(Int32 oldCount, Int32 newCount);

		public void Init(AskOverwrite askOverwrite, NotifyNewCount notifyNewCount, IList<Commit> commitList)
		{
			var gitConfig = Path.Combine(sourceDirectory, ".git");

			if (Directory.Exists(gitConfig))
			{
				var shouldOverwrite = askOverwrite();

				if (!shouldOverwrite)
				{
					var oldCount = commitList.Count;
					removeAlreadyCommited(commitList);
					var newCount = commitList.Count;

					if (oldCount != newCount) notifyNewCount(oldCount, newCount);

					return;
				}

				Directory.Delete(gitConfig, true);
			}

			Run("git", "init");
			File.WriteAllText(alreadyCommitedFile, String.Empty);
		}

		private void removeAlreadyCommited(IList<Commit> commitList)
		{
			if (!File.Exists(alreadyCommitedFile)) return;

			var hashList = File.ReadAllLines(alreadyCommitedFile);

			commitList
				.Where(c => hashList.Contains(c.Hash))
				.ToList()
				.ForEach(c => commitList.Remove(c));
		}

		public void RemakeIgnore()
		{
			var hgIgnore = Path.Combine(sourceDirectory, ".hgignore");

			var ignoreContent =
				File.Exists(hgIgnore)
					? File.ReadAllLines(hgIgnore).ToList()
					: new List<String>();

			ignoreContent.Add(".hg*");

			var gitIgnore = Path.Combine(sourceDirectory, ".gitignore");
            File.WriteAllLines(gitIgnore, ignoreContent);
		}

		public void HandleBranch(Commit commit)
		{
			if (commit.Branch == currentBranch) return;
			
			if (branches.Contains(commit.Branch))
			{
				Run("git", "checkout", commit.Branch);
			}
			else
			{
				Run("git", "checkout", "-b", commit.Branch);
				branches.Add(commit.Branch);
			}

			currentBranch = commit.Branch;
		}

		public void AddAndCommit(Commit commit)
		{
			Run("git", "add .");

			var date = commit.DateTime.ToString(@"yyyy-MM-dd HH:mm:ss (K)");
			var message = commit.Message
				+ Environment.NewLine + Environment.NewLine
				+ $"by [{commit.Author}] in [{date}]";

            Run("git", "commit", $@"-m ""{message}""", "-q");

			File.AppendAllLines(alreadyCommitedFile, new [] { commit.Hash });

			if (!String.IsNullOrEmpty(commit.Tag))
			{
				Run("git", "tag", commit.Tag);
			}
		}
	}
}