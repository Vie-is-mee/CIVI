using System;
using System.Linq;

namespace Civi.Preview
{
    [Serializable] public sealed class DraftBrief
    {
        public string draft_type,title,description;
        public string[] recipient_options;
        public bool preview_before_send,preview_is_not_publication,original_evidence_immutable,only_tribute_can_prepare_board;
    }
    // Presentation request only. TV2 must validate sources, consent, version and command before applying it.
    public sealed class DraftPreviewRequest
    {
        public string DraftType {get;}
        public string Recipient {get;}
        public string Scope=>"internal_review";
        public int Version {get;}
        public string ChoiceId=>Recipient=="held"?"S05_HOLD":Recipient=="nhi"?"S05_SEND_NHI":DraftType=="tribute"?"S05_SEND_GROUP_TRIBUTE":"S05_SEND_GROUP_OTHER";
        public string[] Sources=>DraftType=="tribute"?new[]{"PHOTO_0442"}:DraftType=="paired"?new[]{"PHOTO_S00_GOOD"}:new[]{"PHOTO_S00_BAD_01","PHOTO_S00_BAD_02","PROP_DIAGRAM"};
        public DraftPreviewRequest(string type,string recipient,int version)
        {
            if(!new[]{"tribute","paired","memory"}.Contains(type)||!new[]{"held","nhi","group_four"}.Contains(recipient)||version<1)
                throw new ArgumentException("Unsupported draft preview request");
            DraftType=type;Recipient=recipient;Version=version;
        }
    }
}
