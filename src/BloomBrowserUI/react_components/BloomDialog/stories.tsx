/** @jsx jsx **/
import { jsx, css } from "@emotion/core";

import * as React from "react";
import { storiesOf } from "@storybook/react";
import {
    BloomDialog,
    DialogBottomButtons,
    DialogBottomLeftButtons,
    DialogMiddle,
    DialogTitle
} from "./BloomDialog";
import { Button, CircularProgress } from "@material-ui/core";
import {
    WarningBox,
    DialogCancelButton,
    DialogCloseButton,
    ErrorBox,
    NoteBox
} from "./commonDialogComponents";
import {
    INumberChooserDialogProps,
    NumberChooserDialog
} from "../numberChooserDialog";
import { closeDialog } from "../../bookEdit/editViewFrame";
import {
    normalDialogEnvironmentForStorybook,
    useSetupBloomDialog
} from "./BloomDialogPlumbing";

storiesOf("Bloom Dialog", module)
    .add("Simple Dialog", () => {
        return React.createElement(() => {
            const {
                showDialog,
                closeDialog,
                propsForBloomDialog
            } = useSetupBloomDialog(normalDialogEnvironmentForStorybook);
            // normally here we would assign showDialog to an exported function that
            // other parts of the UI can use to show this dialog. But that doesn't
            // really work here in story-land, so we'll just use it below in a button.
            return (
                <div>
                    <BloomDialog {...propsForBloomDialog}>
                        <DialogTitle title="A Simple <BloomDialog>" />
                        <DialogMiddle
                            css={css`
                                height: 200px;
                                width: 500px;
                                p {
                                    margin-top: 0;
                                }
                            `}
                        >
                            <p>
                                We should have a consistent amount of space
                                between every element and the borders of the
                                dialog box.
                            </p>
                        </DialogMiddle>
                        <DialogBottomButtons>
                            <DialogCloseButton onClick={closeDialog} />
                        </DialogBottomButtons>
                    </BloomDialog>
                    <Button
                        onClick={() => showDialog()}
                        variant={"contained"}
                        color="secondary"
                    >
                        {"Show Dialog"}
                    </Button>
                </div>
            );
        });
    })
    .add("Dialog with the kitchen sink", () => {
        return React.createElement(() => {
            const {
                showDialog,
                closeDialog,
                propsForBloomDialog
            } = useSetupBloomDialog(normalDialogEnvironmentForStorybook);
            return (
                <BloomDialog {...propsForBloomDialog}>
                    <DialogTitle
                        icon="Check In.svg"
                        backgroundColor="#ffffad"
                        color="black"
                        title="A Nice Progress Dialog"
                    >
                        <CircularProgress
                            css={css`
                                margin-left: auto;
                                margin-top: auto;
                                margin-bottom: auto;
                                color: black !important;
                            `}
                            size={20}
                            className={"circle-progress"}
                            color={undefined}
                        />
                    </DialogTitle>
                    <DialogMiddle
                        css={css`
                            height: 300px;
                            width: 500px;
                            p {
                                margin-top: 0;
                            }
                        `}
                    >
                        <p>
                            All three elements in the title bar should be
                            vertically aligned. We should have a consistent
                            amount of space between every element and the
                            borders of the dialog box. This will overflow which
                            should lead to a vertical scroll bar.
                        </p>
                        <WarningBox>Don't step on Superman's cape.</WarningBox>
                        <p>
                            I notice at the moment that the spacing between
                            these paragraphs and special boxes is messed up.
                        </p>
                        <NoteBox>
                            The Broncos will have a winning season some year.
                        </NoteBox>
                        <p>
                            Est anim nisi aute cupidatat elit voluptate ut aute
                            quis esse excepteur. Deserunt irure eiusmod occaecat
                            nisi est exercitation.
                        </p>
                        <ErrorBox>Abandon all hope.</ErrorBox>
                        <p>
                            Ea non consequat irure et elit enim laboris fugiat
                            ipsum. Lorem ipsum velit ut duis ex magna aliquip
                            quis. Magna incididunt ullamco qui in aliquip. Est
                            anim nisi aute cupidatat elit voluptate ut aute quis
                            esse excepteur. Deserunt irure eiusmod occaecat nisi
                            est exercitation. Reprehenderit excepteur excepteur
                            cupidatat nisi esse nisi. Nostrud excepteur irure
                            incididunt nisi velit voluptate velit proident.
                        </p>{" "}
                        <p>
                            Ea non consequat irure et elit enim laboris fugiat
                            ipsum. Lorem ipsum velit ut duis ex magna aliquip
                            quis. Magna incididunt ullamco qui in aliquip. Est
                            anim nisi aute cupidatat elit voluptate ut aute quis
                            esse excepteur. Deserunt irure eiusmod occaecat nisi
                            est exercitation. Reprehenderit excepteur excepteur
                            cupidatat nisi esse nisi. Nostrud excepteur irure
                            incididunt nisi velit voluptate velit proident.
                        </p>
                    </DialogMiddle>
                    <DialogBottomButtons>
                        <DialogBottomLeftButtons>
                            <Button color="primary">
                                Something on the left
                            </Button>
                        </DialogBottomLeftButtons>
                        <Button variant="contained" color="primary">
                            Just Do It
                        </Button>
                        <DialogCancelButton onClick={closeDialog} />
                    </DialogBottomButtons>
                </BloomDialog>
            );
        });
    })
    .add("Number Chooser Dialog", () => {
        const props: INumberChooserDialogProps = {
            min: 2,
            max: 777,
            title: "My Random Chooser Title",
            prompt: "Enter some number from 2 to 777",
            onClick: num => {
                console.log(`We chose ${num}.`);
            },
            dialogEnvironment: normalDialogEnvironmentForStorybook
        };
        return React.createElement(() => {
            return <NumberChooserDialog {...props}></NumberChooserDialog>;
        });
    });
