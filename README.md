# subtitles

a CLI tool that offers the following features:

* Maniuplate time stamps by shifting all of them by a certain number of milliseconds.
* Manipulate all time stamps by shrinking or stretching the duration of time stamps to match video duration
* convert text encoding from windows-1256 to UTF-8 (very useful for Arabic subtitles created on old Windows machines)

# usage

```
	subtitles inputFile <command> <value(s)> [out output_filename]

Examples:
	subtitles the_village.srt utf8 out fixed.srt
	      converts encoding of source file from Arabic windows-1256 to utf-8
	      utf8 encoding is saved to [fixed.srt] file

	subtitles the_village.srt shift 1300
		shift timing in each subtitles lines by adding 1300 milliseconds, and overwrite original file

	subtitles the_village.srt shift 5 00:02:24,415
		the 5th line in the subtitles file should be on screen at the timestamp 00:02:24,415.
		all other lines are shifted according to the found time span difference

	subtitles the_village.srt shift -26000 out the_village_fixed.srt
		shift timing in each subtitles lines by subtracting 26000 milliseconds, and save to the_village_fixed.srt

	subtitles the_village.srt sync 5 643 01:14:23,015
		the 5th line is a reference time and is correct both in file and in movie
		the 643rd line is a out of sync and should be at the timestamp 01:14:23,015
		all other lines will get decrement/increment ratio based on the reference times
```

