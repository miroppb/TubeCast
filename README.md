# TubeCast


**The What**: A service that takes a Youtube Channel (Id or @Handle), and produces a Podcast from the latest videos.

**The Why**: I wanted a quick way to listen to some long videos, without the need to open Youtube and search by Subscriptions. Also, this allows me to have the audio downloaded to my phone beforehand.

**The How**: I use the popular yt-dlp tool to download and convert the audio, and store the files in a web-facing directory.

```
-- Dumping database structure for tubecast
CREATE DATABASE IF NOT EXISTS `tubecast`
USE `tubecast`;

CREATE TABLE IF NOT EXISTS `seen` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `ytid` tinytext NOT NULL,
  `dateseen` datetime NOT NULL,
  `location` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=23 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `settings` (
  `api` tinytext NOT NULL,
  `domain` tinytext NOT NULL,
  `path` text NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
```