using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace GitUpdateTest {
	class Program {
		static void Main(string[] args) {
			Console.Title = "GitUpdate";
			GitUpdate.Init();

			if (GitUpdate.CheckForUpdates()) {
				GitUpdate.RemoveUntrackedFiles();

				Console.Write("Update found, pulling ... ");
				GitUpdate.Update();
				Console.WriteLine("OK");
			}

			GitUpdate.ExecuteRun();
		}
	}

	public static class GitUpdate {
		static string RepoURL;
		static string WorkingDir;
		static string Branch;
		static string Run;

		static Repository CurrentRepo;

		const string GitUpdateCfg = "gitupdate.cfg";

		static bool IsValidConfig() {
			if (string.IsNullOrEmpty(RepoURL) || string.IsNullOrEmpty(WorkingDir) || string.IsNullOrEmpty(Branch))
				return false;

			return true;
		}

		static void LoadCfg() {
			string[] CfgLines = File.ReadAllLines(GitUpdateCfg);

			for (int i = 0; i < CfgLines.Length; i++) {
				int IdxEq = CfgLines[i].IndexOf('=');

				string Key = CfgLines[i].Substring(0, IdxEq).Trim();
				string Val = CfgLines[i].Substring(IdxEq + 1, CfgLines[i].Length - IdxEq - 1).Trim();

				if (Key == "RepoURL")
					RepoURL = Val;
				else if (Key == "WorkingDir")
					WorkingDir = Val;
				else if (Key == "Branch")
					Branch = Val;
				else if (Key == "Run")
					Run = Val;
				else
					throw new InvalidOperationException();
			}
		}

		static void InitRepo() {
			if (!IsValidConfig())
				return;

			try {
				CurrentRepo = new Repository(WorkingDir);
			} catch (RepositoryNotFoundException) {
				if (string.IsNullOrEmpty(Branch))
					throw new InvalidOperationException("Branch name not defined");

				if (!Directory.Exists(WorkingDir))
					Directory.CreateDirectory(WorkingDir);

				Repository.Clone(RepoURL, WorkingDir, new CloneOptions() { BranchName = Branch });
				CurrentRepo = new Repository(WorkingDir);

				Update();
			}
		}

		public static bool CheckForUpdates() {
			if (!IsValidConfig())
				return false;

			Remote RemoteOrigin = CurrentRepo.Network.Remotes["origin"];
			IEnumerable<string> refSpecs = RemoteOrigin.FetchRefSpecs.Select(x => x.Specification);
			Commands.Fetch(CurrentRepo, RemoteOrigin.Name, refSpecs, null, "");

			int? Behind = CurrentRepo.Head.TrackingDetails.BehindBy;
			if (Behind == null)
				return false;

			if (Behind > 0)
				return true;

			return false;
		}

		public static void RemoveUntrackedFiles() {
			CurrentRepo.RemoveUntrackedFiles();
		}

		static void UpdateGitConfig() {
			string NewGitUpdate = Path.Combine(WorkingDir, GitUpdateCfg);

			if (File.Exists(NewGitUpdate)) {
				File.Delete(GitUpdateCfg);
				File.Copy(NewGitUpdate, GitUpdateCfg);
				LoadCfg();
			}
		}

		public static void Update() {
			Signature Merger = new Signature("gitupdate", "gitupdate@local", DateTimeOffset.Now);

			PullOptions Opts = new PullOptions() {
				FetchOptions = new FetchOptions(),
				MergeOptions = new MergeOptions() {
					FileConflictStrategy = CheckoutFileConflictStrategy.Theirs,
					FastForwardStrategy = FastForwardStrategy.Default
				}
			};

			Commands.Pull(CurrentRepo, Merger, Opts);
			UpdateGitConfig();
		}

		public static void ExecuteRun() {
			if (!IsValidConfig())
				return;

			if (string.IsNullOrEmpty(Run))
				return;

			string Exec = Path.Combine(WorkingDir, Run);
			Process.Start(Exec);
		}

		public static void Init() {
			LoadCfg();
			InitRepo();
		}
	}
}
