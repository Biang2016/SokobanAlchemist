/////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Audiokinetic Wwise generated include file. Do not edit.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef __WWISE_IDS_H__
#define __WWISE_IDS_H__

#include <AK/SoundEngine/Common/AkTypes.h>

namespace AK
{
    namespace EVENTS
    {
        static const AkUniqueID BGM_START = 2985635692U;
        static const AkUniqueID BGM_STOP = 2090192256U;
        static const AkUniqueID MUSIC_REGION = 3741135221U;
    } // namespace EVENTS

    namespace STATES
    {
        namespace COMBAT
        {
            static const AkUniqueID GROUP = 2764240573U;

            namespace STATE
            {
                static const AkUniqueID INBOSSCOMBAT = 687614019U;
                static const AkUniqueID INCOMBAT = 3373579172U;
                static const AkUniqueID INELITECOMBAT = 789371013U;
                static const AkUniqueID NONE = 748895195U;
            } // namespace STATE
        } // namespace COMBAT

        namespace COMBAT_ENEMYSPIDERLEG_PHASELEVEL
        {
            static const AkUniqueID GROUP = 2589279669U;

            namespace STATE
            {
                static const AkUniqueID ENEMYSPIDERLEG_PHASE1 = 1652927213U;
                static const AkUniqueID ENEMYSPIDERLEG_PHASE2 = 1652927214U;
                static const AkUniqueID ENEMYSPIDERLEG_PHASE3 = 1652927215U;
                static const AkUniqueID NONE = 748895195U;
            } // namespace STATE
        } // namespace COMBAT_ENEMYSPIDERLEG_PHASELEVEL

        namespace MUSIC_REGIONS
        {
            static const AkUniqueID GROUP = 2106907484U;

            namespace STATE
            {
                static const AkUniqueID DUNGEON = 608898761U;
                static const AkUniqueID EXCAVATION = 2525109375U;
                static const AkUniqueID LAB = 578926554U;
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID OPENWORLD = 2009274547U;
                static const AkUniqueID TEMPLE = 2323193050U;
                static const AkUniqueID TUTORIAL = 3762955427U;
            } // namespace STATE
        } // namespace MUSIC_REGIONS

    } // namespace STATES

    namespace SWITCHES
    {
        namespace COMBAT
        {
            static const AkUniqueID GROUP = 2764240573U;

            namespace SWITCH
            {
                static const AkUniqueID INBOSSCOMBAT = 687614019U;
                static const AkUniqueID INCOMBAT = 3373579172U;
                static const AkUniqueID INELITECOMBAT = 789371013U;
                static const AkUniqueID NONCOMBAT = 1841965078U;
            } // namespace SWITCH
        } // namespace COMBAT

        namespace COMBAT_ENEMYSPIDERLEG_PHASELEVEL
        {
            static const AkUniqueID GROUP = 2589279669U;

            namespace SWITCH
            {
                static const AkUniqueID ENEMYSPIDERLEG_PHASE1 = 1652927213U;
                static const AkUniqueID ENEMYSPIDERLEG_PHASE2 = 1652927214U;
                static const AkUniqueID ENEMYSPIDERLEG_PHASE3 = 1652927215U;
                static const AkUniqueID NONSPIDERCOMBAT = 1624858949U;
            } // namespace SWITCH
        } // namespace COMBAT_ENEMYSPIDERLEG_PHASELEVEL

        namespace DISTANCE_TO_PLAYER
        {
            static const AkUniqueID GROUP = 3917897988U;

            namespace SWITCH
            {
                static const AkUniqueID CLOSE = 1451272583U;
                static const AkUniqueID FAR = 1183803292U;
                static const AkUniqueID MEDIUM = 2849147824U;
            } // namespace SWITCH
        } // namespace DISTANCE_TO_PLAYER

        namespace ENEMYSPIDERLEG_VOLUME_BY_DISTNACE
        {
            static const AkUniqueID GROUP = 3762693077U;

            namespace SWITCH
            {
                static const AkUniqueID CLOSE = 1451272583U;
                static const AkUniqueID FAR = 1183803292U;
                static const AkUniqueID MIDDLE = 1026627430U;
            } // namespace SWITCH
        } // namespace ENEMYSPIDERLEG_VOLUME_BY_DISTNACE

        namespace PLAYER_MOVEMENTSPEED
        {
            static const AkUniqueID GROUP = 2471758783U;

            namespace SWITCH
            {
                static const AkUniqueID DASH = 1942692385U;
                static const AkUniqueID WALK = 2108779966U;
            } // namespace SWITCH
        } // namespace PLAYER_MOVEMENTSPEED

        namespace PLAYER_PICKUP_ITEMTYPE
        {
            static const AkUniqueID GROUP = 3639591801U;

            namespace SWITCH
            {
            } // namespace SWITCH
        } // namespace PLAYER_PICKUP_ITEMTYPE

        namespace SURFACE_TYPE
        {
            static const AkUniqueID GROUP = 4064446173U;

            namespace SWITCH
            {
                static const AkUniqueID CONCRETE = 841620460U;
                static const AkUniqueID DIRT = 2195636714U;
                static const AkUniqueID GRASS = 4248645337U;
                static const AkUniqueID ICE = 344481046U;
            } // namespace SWITCH
        } // namespace SURFACE_TYPE

    } // namespace SWITCHES

    namespace GAME_PARAMETERS
    {
        static const AkUniqueID COMBATLEVEL = 3717892141U;
        static const AkUniqueID DISTANCE_TO_PLAYER = 3917897988U;
        static const AkUniqueID INCOMBAT = 3373579172U;
        static const AkUniqueID INMENU_LOWPASS = 1756231317U;
        static const AkUniqueID MENUSLIDER_MASTER_VOLUME = 1333239961U;
        static const AkUniqueID MENUSLIDER_MUSIC_VOLUME = 2642438544U;
        static const AkUniqueID MUSIC_ENEMYTHEME_VOLUME = 3588190939U;
        static const AkUniqueID PLAYER_HEALTH = 215992295U;
        static const AkUniqueID PLAYER_MOVEMENTSPEED = 2471758783U;
        static const AkUniqueID SFX_ENTITY_VOLUME = 3942311461U;
        static const AkUniqueID SFX_UI_VOLUME = 2698376064U;
        static const AkUniqueID SFX_VOLUME = 1564184899U;
        static const AkUniqueID SPIDERLEGENEMY_COMBAT_INTENSITY = 1190047843U;
    } // namespace GAME_PARAMETERS

    namespace TRIGGERS
    {
        static const AkUniqueID TRIGGER_PLAYERDEATH = 821629565U;
        static const AkUniqueID TRIGGER_QUESTCOMPLETE = 48326457U;
        static const AkUniqueID TRIGGER_TELEPORT = 1218580015U;
        static const AkUniqueID TRIGGER_VICTORY = 1605137596U;
    } // namespace TRIGGERS

    namespace BANKS
    {
        static const AkUniqueID INIT = 1355168291U;
        static const AkUniqueID BGM_BANK = 3734788450U;
        static const AkUniqueID SFX_BANK = 1605876511U;
    } // namespace BANKS

    namespace BUSSES
    {
        static const AkUniqueID AMBIENT = 77978275U;
        static const AkUniqueID AUX = 983310469U;
        static const AkUniqueID BGM_AUDIO_BUS = 341217133U;
        static const AkUniqueID BOXES = 2016903052U;
        static const AkUniqueID ENEMIES = 2242381963U;
        static const AkUniqueID MASTER_AUDIO_BUS = 2392784291U;
        static const AkUniqueID MUSICSTINGER = 1636576504U;
        static const AkUniqueID NONWORLD = 841296678U;
        static const AkUniqueID PLAYER = 1069431850U;
        static const AkUniqueID REGION_DUNGEON = 3334061918U;
        static const AkUniqueID REGION_EXCAVATOR = 1355693303U;
        static const AkUniqueID REGION_LAB = 2754982597U;
        static const AkUniqueID REGION_OPENWORLD = 1863167500U;
        static const AkUniqueID REGION_TEMPLE = 2746966547U;
        static const AkUniqueID REGION_TUTORIAL = 1843703454U;
        static const AkUniqueID SFX_AUDIO_BUS = 3365874942U;
        static const AkUniqueID SFX_ENTITY_AUDIO_BUS = 3663755868U;
        static const AkUniqueID UI = 1551306167U;
        static const AkUniqueID WORLD = 2609808943U;
    } // namespace BUSSES

    namespace AUX_BUSSES
    {
        static const AkUniqueID MUSIC_AUX = 2412664669U;
    } // namespace AUX_BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__
