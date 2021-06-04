using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace SqdHome {
	static class Tasks {
		static object Lock = new object();
		static List<Task> TaskList;

		public static void Init() {
			Console.WriteLine("Initializing tasks");
			TaskList = new List<Task>();
		}

		public static Task GetByName(string Name) {
			lock (Lock) {
				foreach (Task T in TaskList) {
					if (T.Name == Name)
						return T;
				}

				return null;
			}
		}

		public static void Remove(Task T) {
			if (T == null)
				return;

			lock (Lock) {
				TaskList.Remove(T);
			}
		}

		public static void Tick() {
			lock (Lock) {
				DateTime Now = DateTime.Now;

				for (int i = 0; i < TaskList.Count; i++) {
					Task T = TaskList[i];

					if (T.RunAt < Now) {
						if (Program.DEBUG_NAMES)
							Console.WriteLine("Triggering '{0}'", T.Name);

						T.Invoke();
					}

					if (T.Delete) {
						if (Program.DEBUG_NAMES)
							Console.WriteLine("Removing completed task '{0}'", T.Name);

						TaskList.Remove(T);
					}
				}
			}
		}

		public static Task Register(string Name, TaskAction Act) {
			lock (Lock) {
				Task T = GetByName(Name);
				if (T != null) {
					if (Program.DEBUG_NAMES)
						Console.WriteLine("Removing task '{0}'", Name);

					Remove(T);
				}

				if (Program.DEBUG_NAMES)
					Console.WriteLine("Registering task '{0}'", Name);

				T = new Task(Name, Act);
				T.ScheduleNext(DateTime.Now);
				TaskList.Add(T);

				return T;
			}
		}

		public static Task Register(TaskAction Act) {
			return Register(Guid.NewGuid().ToString(), Act);
		}
	}

	delegate void TaskAction(Task This);

	class Task {
		public string Name;
		public TaskAction Action;
		public DateTime RunAt;

		public bool Delete;
		public object Userdata;
		int RepeatSeconds;

		public Task(string Name, TaskAction A) {
			this.Name = Name;
			this.Action = A;

			Delete = false;
			RepeatSeconds = 0;
			Userdata = null;
		}

		public void ScheduleNext(DateTime Time) {
			Delete = false;
			this.RunAt = Time;
		}

		public void ScheduleAfter(int Seconds) {
			ScheduleNext(DateTime.Now + TimeSpan.FromSeconds(Seconds));
		}

		public void ScheduleRepeat(int RepeatSeconds) {
			this.RepeatSeconds = RepeatSeconds;
		}

		public void Invoke() {
			Delete = true;

			if (RepeatSeconds > 0)
				ScheduleAfter(RepeatSeconds);

			Action(this);
		}
	}
}
