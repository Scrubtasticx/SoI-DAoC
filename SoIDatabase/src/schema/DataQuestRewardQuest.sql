-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               10.3.10-MariaDB - mariadb.org binary distribution
-- Server OS:                    Win64
-- HeidiSQL Version:             9.4.0.5125
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- Dumping structure for table soidaoc.dataquestrewardquest
CREATE TABLE IF NOT EXISTS `dataquestrewardquest` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `QuestName` varchar(255) NOT NULL,
  `StartNPC` varchar(100) NOT NULL,
  `StartRegionID` smallint(5) unsigned NOT NULL,
  `StoryText` text DEFAULT NULL,
  `Summary` text DEFAULT NULL,
  `AcceptText` text DEFAULT NULL,
  `QuestGoals` text DEFAULT NULL,
  `GoalType` text DEFAULT NULL,
  `GoalRepeatNo` text DEFAULT NULL,
  `GoalTargetName` text DEFAULT NULL,
  `GoalTargetText` text DEFAULT NULL,
  `StepCount` int(11) DEFAULT NULL,
  `FinishNPC` text DEFAULT NULL,
  `AdvanceText` text DEFAULT NULL,
  `CollectItemTemplate` text DEFAULT NULL,
  `MaxCount` smallint(5) unsigned NOT NULL,
  `MinLevel` tinyint(3) unsigned NOT NULL,
  `MaxLevel` tinyint(3) unsigned NOT NULL,
  `RewardMoney` bigint(20) DEFAULT NULL,
  `RewardXP` bigint(20) DEFAULT NULL,
  `RewardCLXP` bigint(20) DEFAULT NULL,
  `RewardRP` bigint(20) DEFAULT NULL,
  `RewardBP` bigint(20) DEFAULT NULL,
  `OptionalRewardItemTemplates` text DEFAULT NULL,
  `FinalRewardItemTemplates` text DEFAULT NULL,
  `FinishText` text DEFAULT NULL,
  `QuestDependency` text DEFAULT NULL,
  `AllowedClasses` varchar(200) DEFAULT NULL,
  `ClassType` text DEFAULT NULL,
  `XOffset` text DEFAULT NULL,
  `YOffset` text DEFAULT NULL,
  `ZoneID` text DEFAULT NULL,
  `LastTimeRowUpdated` datetime NOT NULL DEFAULT '2000-01-01 00:00:00',
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

-- Data exporting was unselected.
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
