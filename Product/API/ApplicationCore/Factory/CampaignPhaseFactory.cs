﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UpDiddyApi.Models;
using UpDiddyLib.Dto;
using UpDiddyLib.Helpers;

namespace UpDiddyApi.ApplicationCore.Factory
{
    public class CampaignPhaseFactory
    {
     
        public static CampaignPhase CreateCampaignPhase(string PhaseName, string PhaseDescription, int CampaignId)
        {
            return new CampaignPhase()
            {
                CreateDate = DateTime.Now,
                ModifyDate = DateTime.Now,
                IsDeleted = 0,
                CreateGuid = Guid.Empty,
                ModifyGuid = Guid.Empty,
                CampaignPhaseGuid = Guid.NewGuid(),
                Name = PhaseName,
                CampaignId = CampaignId,
                Description = PhaseDescription
            };

        }


        // get a campaign phase by name for the given campaign
        public static CampaignPhase GetCampaignPhaseByName(UpDiddyDbContext db, int CampaignId, string CampaignPhaseName)
        {
            return db.CampaignPhase
                .Where(s => s.IsDeleted == 0 && s.CampaignId == CampaignId &&  s.Name == CampaignPhaseName)
                .FirstOrDefault();
        }


        // todo - this function assumes that the campaign phases will be created in the same chronological order
        // e.g.  as they will be sent.  this needs to be changed to allow the user to specify the chronological 
        // order of the phases irregardless of the order in which they are created

        // get the first phase of the specified campaign
        public static CampaignPhase GetCampaignInitialPhase(UpDiddyDbContext db, int CampaignId)
        {
            return db.CampaignPhase
                .Where(s => s.IsDeleted == 0 && s.CampaignId == CampaignId)
                .OrderBy(s => s.CampaignPhaseId)
                .FirstOrDefault();
        }
        // get the specified campaign by name if possible, if not return the first phase of the campaign
        public static CampaignPhase GetCampaignPhaseByNameOrInitial(UpDiddyDbContext db, int CampaignId, string CampaignPhaseName)
        {
            CampaignPhase campaign = GetCampaignPhaseByName(db, CampaignId, CampaignPhaseName);
            // return the first phase of the campaign if we cannot find the one specified by CampaignPhaseName
            if (campaign == null)
                campaign = GetCampaignInitialPhase(db, CampaignId);
            return campaign;
        }

        // return the most recent campaign phase the given contact has interacted with for the given campaign
        public static CampaignPhase GetContactsLastPhaseInteraction(UpDiddyDbContext db, int CampaignId, int PartnerContactId)
        {
            // get a list of all of the contacts interactions with a campaign ordered by campaign phase id descending
            PartnerContactAction action = db.PartnerContactAction
                .Where(pca => pca.IsDeleted == 0 && pca.PartnerContactId == PartnerContactId && pca.CampaignId == CampaignId)
                .OrderByDescending( a => a.CampaignPhaseId)
                .FirstOrDefault();

            CampaignPhase campaignPhase = null;
            if ( action != null )
            {
                // get the campaign phase associated with the action associated with the most recent 
                // campaign phase
                campaignPhase = db.CampaignPhase
                    .Where(cp => cp.IsDeleted == 0 && cp.CampaignPhaseId == action.CampaignPhaseId)
                    .FirstOrDefault();
            }
            else // if the user does not have any actions, default to the first campaign phase 
            {
                campaignPhase = GetCampaignInitialPhase(db, CampaignId);
            }

            return campaignPhase;
        }


    }
}
