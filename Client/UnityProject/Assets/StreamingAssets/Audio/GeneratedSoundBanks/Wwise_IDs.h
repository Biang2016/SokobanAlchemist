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
        static const AkUniqueID BOXBASE_ONBEINGKICKED = 848561503U;
        static const AkUniqueID BOXBASE_ONBEINGPUSHED = 941375425U;
        static const AkUniqueID FIREBOX_ONBEINGKICKED = 2928738080U;
        static const AkUniqueID FIREBOX_ONBEINGPUSHED = 583630362U;
        static const AkUniqueID LIFEBOX_ONBEINGKICKED = 137324230U;
        static const AkUniqueID WOODENBOX_ONBEINGKICKED = 3669812430U;
        static const AkUniqueID WOODENBOX_ONBEINGPUSHED = 15596056U;
    } // namespace EVENTS

    namespace STATES
    {
        namespace BGM_COMBATSTATE
        {
            static const AkUniqueID GROUP = 3129336127U;

            namespace STATE
            {
                static const AkUniqueID EXPLORING = 1823678183U;
                static const AkUniqueID INBOSSCOMBAT = 687614019U;
                static const AkUniqueID INCOMBAT = 3373579172U;
                static const AkUniqueID INELITECOMBAT = 789371013U;
                static const AkUniqueID INSPIDERLEGCOMBAT_PHASE_1 = 1022704585U;
                static const AkUniqueID INSPIDERLEGCOMBAT_PHASE_2 = 1022704586U;
                static const AkUniqueID INSPIDERLEGCOMBAT_PHASE_3 = 1022704587U;
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID NORMAL = 1160234136U;
            } // namespace STATE
        } // namespace BGM_COMBATSTATE

        namespace BGM_THEME
        {
            static const AkUniqueID GROUP = 3258568639U;

            namespace STATE
            {
                static const AkUniqueID EXCAVATION = 2525109375U;
                static const AkUniqueID LAB = 578926554U;
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID OPENWORLD = 2009274547U;
                static const AkUniqueID TEMPLE = 2323193050U;
                static const AkUniqueID TUTORIAL = 3762955427U;
            } // namespace STATE
        } // namespace BGM_THEME

    } // namespace STATES

    namespace BANKS
    {
        static const AkUniqueID INIT = 1355168291U;
        static const AkUniqueID BGM_BANK = 3734788450U;
        static const AkUniqueID SFX_BANK = 1605876511U;
    } // namespace BANKS

    namespace BUSSES
    {
        static const AkUniqueID BGM_AUDIO_BUS = 341217133U;
        static const AkUniqueID MASTER_AUDIO_BUS = 2392784291U;
        static const AkUniqueID SFX_AUDIO_BUS = 3365874942U;
        static const AkUniqueID SFX_ENTITY_AUDIO_BUS = 3663755868U;
        static const AkUniqueID SFX_UI_AUDIO_BUS = 3991820723U;
    } // namespace BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__
