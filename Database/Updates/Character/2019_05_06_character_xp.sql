ALTER TABLE `character`
ADD COLUMN `xp` int(10) unsigned NOT NULL DEFAULT '0' AFTER `activeSpec`,
ADD COLUMN `restXp` int(10) unsigned NOT NULL DEFAULT '0' AFTER `xp`;

UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 655 WHERE `b`.`stat` = 10 and `b`.`value` = 2;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 1710 WHERE `b`.`stat` = 10 and `b`.`value` = 3;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 2860 WHERE `b`.`stat` = 10 and `b`.`value` = 4;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 4320 WHERE `b`.`stat` = 10 and `b`.`value` = 5;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 6280 WHERE `b`.`stat` = 10 and `b`.`value` = 6;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 8920 WHERE `b`.`stat` = 10 and `b`.`value` = 7;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 12510 WHERE `b`.`stat` = 10 and `b`.`value` = 8;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 17330 WHERE `b`.`stat` = 10 and `b`.`value` = 9;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 23860 WHERE `b`.`stat` = 10 and `b`.`value` = 10;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 32470 WHERE `b`.`stat` = 10 and `b`.`value` = 11;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 43830 WHERE `b`.`stat` = 10 and `b`.`value` = 12;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 58570 WHERE `b`.`stat` = 10 and `b`.`value` = 13;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 77520 WHERE `b`.`stat` = 10 and `b`.`value` = 14;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 101530 WHERE `b`.`stat` = 10 and `b`.`value` = 15;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 131820 WHERE `b`.`stat` = 10 and `b`.`value` = 16;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 169380 WHERE `b`.`stat` = 10 and `b`.`value` = 17;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 215750 WHERE `b`.`stat` = 10 and `b`.`value` = 18;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 272350 WHERE `b`.`stat` = 10 and `b`.`value` = 19;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 340950 WHERE `b`.`stat` = 10 and `b`.`value` = 20;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 423300 WHERE `b`.`stat` = 10 and `b`.`value` = 21;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 521770 WHERE `b`.`stat` = 10 and `b`.`value` = 22;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 638250 WHERE `b`.`stat` = 10 and `b`.`value` = 23;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 775580 WHERE `b`.`stat` = 10 and `b`.`value` = 24;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 936310 WHERE `b`.`stat` = 10 and `b`.`value` = 25;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 1123510 WHERE `b`.`stat` = 10 and `b`.`value` = 26;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 1340160 WHERE `b`.`stat` = 10 and `b`.`value` = 27;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 1590200 WHERE `b`.`stat` = 10 and `b`.`value` = 28;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 1876730 WHERE `b`.`stat` = 10 and `b`.`value` = 29;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 2204290 WHERE `b`.`stat` = 10 and `b`.`value` = 30;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 2576890 WHERE `b`.`stat` = 10 and `b`.`value` = 31;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 2999270 WHERE `b`.`stat` = 10 and `b`.`value` = 32;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 3475990 WHERE `b`.`stat` = 10 and `b`.`value` = 33;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 4012950 WHERE `b`.`stat` = 10 and `b`.`value` = 34;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 4614770 WHERE `b`.`stat` = 10 and `b`.`value` = 35;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 5288090 WHERE `b`.`stat` = 10 and `b`.`value` = 36;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 6038720 WHERE `b`.`stat` = 10 and `b`.`value` = 37;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 6873450 WHERE `b`.`stat` = 10 and `b`.`value` = 38;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 7798740 WHERE `b`.`stat` = 10 and `b`.`value` = 39;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 8822880 WHERE `b`.`stat` = 10 and `b`.`value` = 40;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 9952300 WHERE `b`.`stat` = 10 and `b`.`value` = 41;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 11196140 WHERE `b`.`stat` = 10 and `b`.`value` = 42;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 12562350 WHERE `b`.`stat` = 10 and `b`.`value` = 43;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 14060140 WHERE `b`.`stat` = 10 and `b`.`value` = 44;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 15698220 WHERE `b`.`stat` = 10 and `b`.`value` = 45;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 17487670 WHERE `b`.`stat` = 10 and `b`.`value` = 46;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 19437010 WHERE `b`.`stat` = 10 and `b`.`value` = 47;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 21558320 WHERE `b`.`stat` = 10 and `b`.`value` = 48;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 23862010 WHERE `b`.`stat` = 10 and `b`.`value` = 49;
UPDATE `character` a JOIN `character_stats` `b` ON `a`.`id`=`b`.`id` SET `a`.`xp` = 26360090 WHERE `b`.`stat` = 10 and `b`.`value` = 50;
