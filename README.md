# LaunchAsDate
*Changes the system date, launches the executable and sets the system date
back.*

**LaunchAsDate** is a compact tool designed to launch a program at a specific
date and time determined by the user. It operates by altering the system's date,
initiating the program, and then reverting the system date to its original
setting. The date and the time is adjusted to the user's desired value for as
brief a period as necessary to ensure the application functions correctly, while
also avoiding any potential conflicts that could arise from changing the date
and time for an extended duration.

**It's essential to avoid running multiple instances of the program in the mode
that launches the target application with a different date and time.** This
precaution is necessary to prevent another instance from changing the system
date and time to a different value while it is temporarily modified by the first
instance.

The application does not need to be installed on the target computer. It only
necessitates the **.NET Framework** to operate. The specific target framework is
the **4.0 Client Profile**.

**LaunchAsDate** is released under the **MIT license**.
