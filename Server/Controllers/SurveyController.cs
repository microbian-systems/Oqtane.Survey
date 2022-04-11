using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Survey.Models;
using Oqtane.Survey.Repository;
using Oqtane.Repository;
using Oqtane.Survey.Server.Repository;
using Oqtane.Controllers;

namespace Oqtane.Survey.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class SurveyController : ModuleControllerBase
    {
        private readonly ISurveyRepository _SurveyRepository;
        private readonly IUserRepository _users;

        public SurveyController(ISurveyRepository SurveyRepository, IUserRepository users, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _SurveyRepository = SurveyRepository;
            _users = users;
        }

        // GET: api/<controller>?moduleid=x
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public IEnumerable<Models.Survey> Get(string moduleid)
        {
            var colSurveys = _SurveyRepository.GetAllSurveysByModule(int.Parse(moduleid));
            return ConvertToSurveys(colSurveys);
        }

        // GET api/<controller>?/5
        [HttpGet("{id}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public Models.Survey Get(int id)
        {
            var objSurvey = _SurveyRepository.GetSurvey(id);

            Models.Survey Survey = ConvertToSurvey(objSurvey);

            if (Survey != null && Survey.ModuleId != _authEntityId[EntityNames.Module])
            {
                Survey = null;
            }

            return Survey;
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public Models.Survey Post([FromBody] Models.Survey Survey)
        {
            if (ModelState.IsValid && Survey.ModuleId == _authEntityId[EntityNames.Module])
            {
                // Get User
                var User = _users.GetUser(this.User.Identity.Name);

                // Add User to Survey object
                Survey.UserId = User.UserId;

                Survey = ConvertToSurvey(_SurveyRepository.CreateSurvey(Survey));
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Survey Added {Survey}", Survey);
            }
            return Survey;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public Models.Survey Put(int id, [FromBody] Models.Survey Survey)
        {
            if (ModelState.IsValid && Survey.ModuleId == _authEntityId[EntityNames.Module])
            {
                Survey = ConvertToSurvey(_SurveyRepository.UpdateSurvey(Survey));
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Survey Updated {Survey}", Survey);
            }
            return Survey;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public void Delete(int id)
        {
            var objSurvey = _SurveyRepository.GetSurvey(id);

            Models.Survey Survey = ConvertToSurvey(objSurvey);

            if (Survey != null && Survey.ModuleId == _authEntityId[EntityNames.Module])
            {
                // Delete all Survey Items
                if (Survey.SurveyItem != null)
                {
                    foreach (var item in Survey.SurveyItem)
                    {
                        bool boolDeleteSurveyItemResult = _SurveyRepository.DeleteSurveyItem(item.Id);

                        if (boolDeleteSurveyItemResult)
                        {
                            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Survey Item Deleted {item.Id}", item.Id);
                        }
                        else
                        {
                            _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Error: Survey Item *NOT* Deleted {item.Id}", item.Id);
                        }
                    }
                }

                bool boolResult = _SurveyRepository.DeleteSurvey(id);

                if (boolResult)
                {
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Survey Deleted {id}", id);
                }
                else
                {
                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Error: Survey *NOT* Deleted {id}", id);
                }
            }
        }

        // Utility
        #region private IEnumerable<Models.Survey> ConvertToSurveys(List<OqtaneSurvey> colOqtaneSurveys)
        private IEnumerable<Models.Survey> ConvertToSurveys(List<OqtaneSurvey> colOqtaneSurveys)
        {
            List<Models.Survey> colSurveyCollection = new List<Models.Survey>();

            foreach (var objOqtaneSurvey in colOqtaneSurveys)
            {
                // Convert to Survey
                Models.Survey objAddSurvey = ConvertToSurvey(objOqtaneSurvey);

                // Add to Collection
                colSurveyCollection.Add(objAddSurvey);
            }

            return colSurveyCollection;
        }
        #endregion

        #region private static Models.Survey ConvertToSurvey(OqtaneSurvey objOqtaneSurvey)
        private Models.Survey ConvertToSurvey(OqtaneSurvey objOqtaneSurvey)
        {
            if(objOqtaneSurvey == null)
            {
                return new Models.Survey();
            }

            // Create new Object
            Models.Survey objAddSurvey = new Models.Survey();

            objAddSurvey.SurveyId = objOqtaneSurvey.SurveyId;
            objAddSurvey.ModuleId = objOqtaneSurvey.ModuleId;
            objAddSurvey.SurveyName = objOqtaneSurvey.SurveyName;
            objAddSurvey.CreatedBy = objOqtaneSurvey.CreatedBy;
            objAddSurvey.CreatedOn = objOqtaneSurvey.CreatedOn;
            objAddSurvey.ModifiedBy = objOqtaneSurvey.ModifiedBy;
            objAddSurvey.ModifiedOn = objOqtaneSurvey.ModifiedOn;
            if (objOqtaneSurvey.UserId != null)
            {
                objAddSurvey.UserId = objOqtaneSurvey.UserId.Value;
            }

            // Create new Collection
            objAddSurvey.SurveyItem = new List<SurveyItem>();

            foreach (OqtaneSurveyItem objOqtaneSurveyItem in objOqtaneSurvey.OqtaneSurveyItem)
            {
                // Create new Object
                Models.SurveyItem objAddSurveyItem = new SurveyItem();

                objAddSurveyItem.Id = objOqtaneSurveyItem.Id;
                objAddSurveyItem.ItemLabel = objOqtaneSurveyItem.ItemLabel;
                objAddSurveyItem.ItemType = objOqtaneSurveyItem.ItemType;
                objAddSurveyItem.ItemValue = objOqtaneSurveyItem.ItemValue;
                objAddSurveyItem.Position = objOqtaneSurveyItem.Position;
                objAddSurveyItem.Required = objOqtaneSurveyItem.Required;
                objAddSurveyItem.SurveyChoiceId = objOqtaneSurveyItem.SurveyChoiceId;

                // Create new Collection
                objAddSurveyItem.SurveyItemOption = new List<SurveyItemOption>();

                foreach (OqtaneSurveyItemOption objOqtaneSurveyItemOption in objOqtaneSurveyItem.OqtaneSurveyItemOption)
                {
                    // Create new Object
                    Models.SurveyItemOption objAddSurveyItemOption = new SurveyItemOption();

                    objAddSurveyItemOption.Id = objOqtaneSurveyItemOption.Id;
                    objAddSurveyItemOption.OptionLabel = objOqtaneSurveyItemOption.OptionLabel;

                    // Add to Collection
                    objAddSurveyItem.SurveyItemOption.Add(objAddSurveyItemOption);
                }

                // Add to Collection
                objAddSurvey.SurveyItem.Add(objAddSurveyItem);
            }

            return objAddSurvey;
        } 
        #endregion
    }
}
