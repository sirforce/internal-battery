﻿import React from 'react';
 

class ResumeMerge extends React.Component {
 
    constructor(props) {
        super(props)
        this.state = { 
            processing: false,
            questions: null,
            hasQuestions: false
        };
        this.onDoParseMerge = this.onDoParseMerge.bind(this);    
}



    spinner() { 

        if (!this.state.processing)
            return;

        return (<div className="d-inline">
            <span className="loading-spinner spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            <span className="loading-spinner sr-only">Loading...</span>
        </div>);
    }


    onDoParseMerge(e) {
        console.log("onDoParseMerge detail = " + e.detail);            
        CareerCircleAPI.getResumeParseMergeQuestionnaire(e.detail)
            .then((response) => {
                this.setState({ questions: response });
                this.setState({ hasQuestions: true });
                $("#ResumeMergeModal").modal();
                //alert(this.state.questions.data.resumeParseGuid )
         
            })
            .catch((err) => {
                ToastService.error('Unable to locate profile merge data.');
            });
    }

 
    formatQuestions(questions) {
        let rval = <div className="profile-edit-modal-header-container">
            <ul className="list-group">
                {
                    questions.map((o, index) =>
                        (
                            <li className="list-group-item border-0" key={o.resumeParseResultGuid}>
                                <div> {o.prompt} </div>
                                <div className="ml-3">
                                    <input type="radio" name={"rb_" + o.resumeParseResultGuid} id={"rb_existing_" + o.resumeParseResultGuid}  value="existing" defaultChecked />  {o.existingValue}
                                </div>
                                <div className="ml-3">
                                    <input type="radio" name={"rb_" + o.resumeParseResultGuid} id={"rb_parsed_" + o.resumeParseResultGuid} value="parsed" />  {o.parsedValue}
                                </div>
                                <div className="ml-3">
                                    <input type="radio" name={"rb_" + o.resumeParseResultGuid} id={"rb_neither_" + o.resumeParseResultGuid}  value="neither" />  Neither
                                </div>
                            </li>
                        ))
                }
            </ul>
        </div>
        return rval;
    }


    formatSkillsGood(skills) {
        let rval = <div className="profile-edit-modal-header-container">
            <h5> Which of the following skills do you have? </h5>
            <ul className="list-group">
                {
                    skills.map((o, index) =>
                        (
                            <li className="list-group-item border-0 p-0 " key={o.resumeParseResultGuid}>
                                <div className="ml-3 h6">
                                    <input type="checkbox" name={"chk_" + o.resumeParseResultGuid} id={"chk_" + o.resumeParseResultGuid} value={o.parsedValue} />  {o.existingValue}
                                </div>
                            </li>
                        ))
                }
            </ul>
        </div>
        return rval;
    }

    formatSkills(skills) {
        let rval = <div className="profile-edit-modal-header-container">
            <h5> Which of the following skills do you have? </h5>
            <div className="">
            
                {
                    skills.map((o, index) =>
                        (
                            <span className="ml-3 h6" style={{ }} key={o.resumeParseResultGuid}>
                                <label>   <input type="checkbox" name={"chk_" + o.resumeParseResultGuid} id={"chk_" + o.resumeParseResultGuid} value={o.parsedValue} />  {o.existingValue}  </label>
                                </span>                            
                        ))
                    }
           
            </div>
        </div>
        return rval;
    }


 



     
    displayWait() {
        this.setState({ processing: true })
    }

    componentDidMount() {
        window.addEventListener('onDoParseMerge', this.onDoParseMerge);
        console.log("--------------------------------- Did Mount ----------------------------------   ")
    }

    render() {
 
        let contactQuestions = null;
        let workHistoryQuestions = null;
        let educationHistoryQuestions = null;
        let skillQuestions = null;
        let parseGuid = "";

        if ( this.state.hasQuestions == true )
            parseGuid = this.state.questions.data.resumeParseGuid;

        if (this.state.hasQuestions == true && this.state.questions.data.contactQuestions.length > 0  )
            contactQuestions = this.formatQuestions(this.state.questions.data.contactQuestions);  

        if (this.state.hasQuestions == true && this.state.questions.data.workHistoryQuestions.length > 0)
            workHistoryQuestions = this.formatQuestions(this.state.questions.data.workHistoryQuestions);  

        if (this.state.hasQuestions == true && this.state.questions.data.educationHistoryQuestions.length > 0)
            educationHistoryQuestions = this.formatQuestions(this.state.questions.data.educationHistoryQuestions);

        if (this.state.hasQuestions == true && this.state.questions.data.skills.length > 0)
            skillQuestions = this.formatSkills(this.state.questions.data.skills);

 
  
        return (
            <form action={"/Home/resume-merge/" + parseGuid}  method="post" id="ResumeMergeForm">   
            <div className="modal fade" id="ResumeMergeModal" tabIndex="-1" role="dialog" aria-hidden="true">
                <div className="modal-dialog" role="document">                
                    <div className="modal-content">
                        <div className="modal-body">
                            <div className="profile-edit-modal-header-container">
                                <h5> Please clarify the following: </h5>
                            </div>            
                            {
                                contactQuestions
                            }
                            {
                                workHistoryQuestions
                            }
                            {
                                educationHistoryQuestions
                            }
                            {
                                skillQuestions
                            }

                            <div className="modal-footer">

                                    <button type="button" className="btn btn-secondary" data-dismiss="modal">Close</button>     
                                    <button id="btnResumeMerge" form="ResumeMergeForm" type="submit" className="btn btn-primary" onClick={() => this.displayWait()} >Save {this.spinner()} </button>
                            </div>
                        </div>
                    </div>
                </div>
  
                </div>
                </form>
        );
    }
}

export default ResumeMerge;